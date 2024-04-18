using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Weapons;
using Dev.Weapons.StaticData;
using Fusion;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Zenject;

namespace Dev.Infrastructure
{
    public class PlayersSpawner : NetworkContext
    {
        [SerializeField] private InputService _inputServicePrefab;
        [SerializeField] private CameraController _cameraControllerPrefab;
        [SerializeField] private PlayerBase _playerBasePrefab;

        public Subject<PlayerSpawnEventContext> PlayerSpawned { get; } = new Subject<PlayerSpawnEventContext>();
        public Subject<PlayerSpawnEventContext> CharacterSpawned { get; } = new Subject<PlayerSpawnEventContext>();
        public Subject<PlayerRef> CharacterDeSpawned { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> PlayerDeSpawned { get; } = new Subject<PlayerRef>();
        public Dictionary<PlayerRef, List<NetworkObject>> PlayerServices => _playerServices;
        [Networked] private NetworkDictionary<PlayerRef, PlayerBase> PlayersBase { get; }

        public List<PlayerCharacter> Players => PlayersBase.Select(x => x.Value.PlayerCharacterInstance).ToList();

        public int PlayersCount => PlayersBase.Count;

        private Dictionary<PlayerRef, List<NetworkObject>> _playerServices =
            new Dictionary<PlayerRef, List<NetworkObject>>();

        private TeamsService _teamsService;
        private PopUpService _popUpService;
        private CharactersDataContainer _charactersDataContainer;
        private PlayersHealthService _playersHealthService;

        [Inject]
        private void Init(TeamsService teamsService, PopUpService popUpService,
            CharactersDataContainer charactersDataContainer, PlayersHealthService playersHealthService)
        {
            _playersHealthService = playersHealthService;
            _charactersDataContainer = charactersDataContainer;
            _popUpService = popUpService;
            _teamsService = teamsService;
        }

        public void ChooseCharacterClass(PlayerRef playerRef)
        {
            RPC_GetCharacterClassAndSpawn(playerRef);
        }

        [Rpc]
        private void RPC_GetCharacterClassAndSpawn([RpcTarget] PlayerRef playerRef)
        {
            _popUpService.TryGetPopUp<CharacterChooseMenu>(out var characterChooseMenu);

            characterChooseMenu.StartChoosingCharacter((characterClass =>
            {
                Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe((l =>
                {
                    _popUpService.HidePopUp<CharacterChooseMenu>();
                    _popUpService.ShowPopUp<HUDMenu>();
                }));

                SpawnPlayerByCharacter(characterClass, playerRef, Runner);
            }));
        }

        private void SpawnPlayerByCharacter(CharacterClass characterClass, PlayerRef playerRef,
            NetworkRunner networkRunner)
        {
            Debug.Log($"Player {playerRef} chose {characterClass}");

            SpawnPlayer(playerRef, characterClass, networkRunner);
        }

        public PlayerCharacter SpawnPlayer(PlayerRef playerRef, CharacterClass characterClass,
            NetworkRunner networkRunner,
            bool firstSpawn = true)
        {
            if (firstSpawn)
            {
                AssignTeam(playerRef);

                PlayerBase playerBase = networkRunner.Spawn(_playerBasePrefab, null, null, playerRef);
                playerBase.Object.AssignInputAuthority(playerRef);

                RPC_AddPlayer(playerRef, playerBase);
            }
            else
            {
                DespawnPlayer(playerRef, false);
            }

            CharacterData characterData = _charactersDataContainer.GetCharacterDataByClass(characterClass);

            PlayerCharacter playerCharacterPrefab = characterData._playerCharacterPrefab;

            TeamSide teamSide = _teamsService.GetPlayerTeamSide(playerRef);
            Vector3 spawnPos = GetSpawnPosByTeam(teamSide);

            PlayerCharacter playerCharacter = networkRunner.Spawn(playerCharacterPrefab, spawnPos,
                quaternion.identity, playerRef);

            NetworkObject playerNetObj = playerCharacter.Object;

            PlayersBase[playerRef].PlayerCharacterInstance = playerCharacter;

            playerNetObj.RequestStateAuthority();
            playerNetObj.AssignInputAuthority(playerRef);
            networkRunner.SetPlayerObject(playerRef, playerNetObj);

            PlayerManager.AddPlayer(playerCharacter);

            playerCharacter.PlayerController.RPC_Init(characterData.CharacterStats.MoveSpeed,
                characterData.CharacterStats.ShootThreshold, characterData.CharacterStats.SpeedLowerSpeed);

            playerCharacter.PlayerController.SetAllowToMove(true);
            playerCharacter.PlayerController.SetAllowToShoot(true);

            playerCharacter.Init(characterClass);

            if (firstSpawn)
            {
                _playerServices.Add(playerRef, new List<NetworkObject>());

                SetInputService(playerRef, networkRunner);
                SetCamera(playerRef, playerCharacter, networkRunner);
            }
            else
            {
                UpdatePlayerCharacter(playerCharacter, playerRef);
                CameraController cameraController = GetPlayerCameraController(playerRef);
                cameraController.FastSetOnTarget();
                cameraController.SetFollowState(true);
            }

            var playerName = $"Player №{playerNetObj.InputAuthority.PlayerId}";
            playerCharacter.RPC_SetName(playerName);

            //RespawnPlayer(playerRef);

            RPC_OnPlayerSpawnedInvoke(playerCharacter);

            //LoadWeapon(player);

            ColorTeamBanner(playerRef);

            return playerCharacter;
        }

        public void DespawnPlayer(PlayerRef playerRef, bool isLeftFromSession)
        {
            PlayerCharacter playerCharacter = GetPlayer(playerRef);

            Runner.Despawn(playerCharacter.Object);

            CharacterDeSpawned.OnNext(playerRef);

            GetPlayerCameraController(playerRef).SetFollowState(false);

            if (isLeftFromSession)
            {
                var playerServices = FindObjectsOfType<PlayerService>(true);
                for (var index = playerServices.Length - 1; index >= 0; index--)
                {
                    PlayerService service = playerServices[index];
                    if (service.Object.InputAuthority == playerRef)
                    {
                        Runner.Despawn(service.Object);
                        Destroy(service.gameObject);
                        Debug.Log($"Despawned service {service.name}", service);
                    }
                }

                _teamsService.RemoveFromTeam(playerRef);

                PlayersBase.Remove(playerRef);
            }

            PlayerManager.RemovePlayer(playerCharacter);
        }

        private void UpdatePlayerCharacter(PlayerCharacter character, PlayerRef playerRef)
        {
            CameraController cameraController = GetPlayerCameraController(playerRef);

            cameraController.SetupTarget(character.transform);
        }

        [Rpc]
        private void RPC_AddPlayer(PlayerRef playerRef, PlayerBase playerBase)
        {
            PlayersBase.Add(playerRef, playerBase);
        }

        private void ColorTeamBanner(PlayerRef playerRef)
        {
            TeamSide teamSide = _teamsService.GetPlayerTeamSide(playerRef);

            Color color = Color.red;

            switch (teamSide)
            {
                case TeamSide.Blue:
                    color = Color.blue;

                    break;
                case TeamSide.Red:
                    color = Color.red;
                    break;
            }

            GetPlayer(playerRef).PlayerView.RPC_SetTeamColor(color);
        }

        private void AssignTeam(PlayerRef playerRef)
        {
            bool doPlayerHasTeam = _teamsService.DoPlayerHasTeam(playerRef);

            if (doPlayerHasTeam) return;

            TeamSide teamSide = TeamSide.Blue;

            if (PlayersCount % 2 == 0)
            {
                teamSide = TeamSide.Red;
            }

            _teamsService.AssignForTeam(playerRef, teamSide);
        }

        private void SetCamera(PlayerRef playerRef, PlayerCharacter playerCharacter, NetworkRunner networkRunner)
        {
            CameraController cameraController = networkRunner.Spawn(_cameraControllerPrefab,
                playerCharacter.transform.position,
                Quaternion.identity,
                playerRef);

            cameraController.SetFollowState(true);
            cameraController.Object.RequestStateAuthority();
            _playerServices[playerRef].Add(cameraController.Object);
        }

        private void SetInputService(PlayerRef playerRef, NetworkRunner networkRunner)
        {
            InputService inputService =
                networkRunner.Spawn(_inputServicePrefab, Vector3.zero, Quaternion.identity, playerRef);

            inputService.Object.RequestStateAuthority();

            _playerServices[playerRef].Add(inputService.Object);
        }

        public void SetPlayerActiveState(PlayerRef playerRef, bool isOn)
        {
            PlayerCharacter playerCharacter = GetPlayer(playerRef);

            playerCharacter.gameObject.SetActive(isOn);

            playerCharacter.PlayerController.SetAllowToMove(isOn);
            playerCharacter.PlayerController.SetAllowToShoot(isOn);
        }

        public void RespawnPlayerCharacter(PlayerRef playerRef)
        {
            _playersHealthService.RestorePlayerHealth(playerRef);

            TeamSide playerTeamSide = _teamsService.GetPlayerTeamSide(playerRef);

            var spawnPoints = LevelService.Instance.CurrentLevel.GetSpawnPointsByTeam(playerTeamSide);

            SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            PlayerCharacter playerCharacter = GetPlayer(playerRef);

            playerCharacter.RPC_SetPos(spawnPoint.transform.position);

            playerCharacter.PlayerController.SetAllowToMove(true);
            playerCharacter.PlayerController.SetAllowToShoot(true);

            playerCharacter.Collider2D.enabled = true;

            ColorTeamBanner(playerRef);
        }

        private void LoadWeapon(PlayerCharacter playerCharacter)
        {
            var weaponSetupContext = new WeaponSetupContext(WeaponType.Rifle);
            playerCharacter.WeaponController.Init(weaponSetupContext);
        }

        public PlayerCharacter GetPlayer(PlayerRef playerRef)
        {
            return PlayersBase[playerRef].PlayerCharacterInstance;
        }

        public CameraController GetPlayerCameraController(PlayerRef playerRef)
        {
            List<NetworkObject> playerService = _playerServices[playerRef];

            CameraController cameraController = playerService.First(x => x.GetComponent<CameraController>() != null)
                .GetComponent<CameraController>();

            return cameraController;
        }

        public Vector3 GetPlayerPos(PlayerRef playerRef) => GetPlayer(playerRef).transform.position;

        public Vector3 GetSpawnPosByTeam(TeamSide teamSide)
        {
            var spawnPoints = LevelService.Instance.CurrentLevel.GetSpawnPointsByTeam(teamSide);

            SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            return spawnPoint.transform.position;
        }

        [Rpc]
        private void RPC_OnPlayerSpawnedInvoke(PlayerCharacter playerCharacter)
        {
            var spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.PlayerRef = playerCharacter.Object.InputAuthority;
            spawnEventContext.Transform = playerCharacter.transform;
            spawnEventContext.CharacterClass = playerCharacter.CharacterClass;

            // Debug.Log($"[RPC] Player spawned");
            PlayerSpawned.OnNext(spawnEventContext);
        }
    }
}