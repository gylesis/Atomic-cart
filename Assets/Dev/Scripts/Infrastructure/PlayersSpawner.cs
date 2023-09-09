using System;
using System.Collections.Generic;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Dev.Weapons;
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

        [Networked] public int PlayersCount { get; set; }

        private Dictionary<PlayerRef, Player> _players = new Dictionary<PlayerRef, Player>();

        public Subject<PlayerSpawnEventContext> Spawned { get; } = new Subject<PlayerSpawnEventContext>();
        public Subject<PlayerRef> DeSpawned { get; } = new Subject<PlayerRef>();

        private Dictionary<PlayerRef, List<NetworkObject>> _playerServices =
            new Dictionary<PlayerRef, List<NetworkObject>>();

        public IReadOnlyCollection<Player> Players => _players.Values;

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

        public void SpawnPlayerByCharacterClass(PlayerRef playerRef)
        {
            RPC_GetCharacterClass(playerRef);
        }

        [Rpc]
        private void RPC_GetCharacterClass([RpcTarget] PlayerRef playerRef)
        {
            _popUpService.TryGetPopUp<CharacterChooseMenu>(out var characterChooseMenu);

            characterChooseMenu.StartChoosingCharacter((characterClass =>
            {
                Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe((l =>
                {
                    _popUpService.HidePopUp<CharacterChooseMenu>();
                }));

                RPC_SpawnPlayerByCharacter(characterClass, playerRef);
            }));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_SpawnPlayerByCharacter(CharacterClass characterClass, PlayerRef playerRef)
        {
            Debug.Log($"Player {playerRef} chose {characterClass}");

            SpawnPlayer(playerRef, characterClass);
        }

        public Player SpawnPlayer(PlayerRef playerRef, CharacterClass characterClass)
        {
            AssignTeam(playerRef);

            CharacterData characterData = _charactersDataContainer.GetCharacterDataByClass(characterClass);

            Player playerPrefab = characterData.PlayerPrefab;

            TeamSide teamSide = _teamsService.GetPlayerTeamSide(playerRef);
            Vector3 spawnPos = GetSpawnPos(teamSide);

            Player player = Runner.Spawn(playerPrefab, spawnPos,
                quaternion.identity, playerRef);

            PlayerManager.AddPlayer(player);
            
            player.PlayerController.RPC_Init(characterData.CharacterStats.MoveSpeed,
                characterData.CharacterStats.ShootThreshold, characterData.CharacterStats.SpeedLowerSpeed);

            player.PlayerController.AllowToMove = true;
            player.PlayerController.AllowToShoot = true;
            
            player.Init(characterClass);

            NetworkObject playerNetObj = player.Object;
            Runner.SetPlayerObject(playerRef, playerNetObj);

            _playerServices.Add(playerRef, new List<NetworkObject>());

            SetInputService(playerRef);
            SetCamera(playerRef, player);

            var playerName = $"Player №{playerNetObj.InputAuthority.PlayerId}";
            player.RPC_SetName(playerName);

            PlayersCount++;

            _players.Add(playerRef, player);

            //RespawnPlayer(playerRef);

            RPC_OnPlayerSpawnedInvoke(player);

            LoadWeapon(player);

            ColorTeamBanner(playerRef);

            return player;
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

            _players[playerRef].PlayerView.RPC_SetTeamColor(color);
        }

        /*public Player SpawnPlayer(PlayerRef playerRef)
        {
            var playersLength = PlayersCount;

            var playerNetObj = Runner.Spawn(_playerPrefab, Vector2.zero + Vector2.right * playersLength,
                quaternion.identity, playerRef);
            var player = playerNetObj.GetComponent<Player>();

            Runner.SetPlayerObject(playerRef, playerNetObj);

            _playerServices.Add(playerRef, new List<NetworkObject>());

            SetInputService(playerRef);
            SetCamera(playerRef, player);

            var playerName = $"Player №{playerNetObj.InputAuthority.PlayerId}";
            player.RPC_SetName(playerName);

            PlayersCount++;

            _players.Add(playerRef, player);

            AssignTeam(playerRef);

            RespawnPlayer(playerRef);

            RPC_OnPlayerSpawnedInvoke(player);

            LoadWeapon(player);

            return player;
        }*/

        private void LoadWeapon(Player player)
        {
            //  var weaponSetupContext = new WeaponSetupContext("AkWeapon");
            //  player.WeaponController.Init(weaponSetupContext);
        }

        private void AssignTeam(PlayerRef playerRef)
        {
            TeamSide teamSide = TeamSide.Blue;

            if (_players.Count % 2 == 0)
            {
                teamSide = TeamSide.Red;
            }

            _teamsService.AssignForTeam(playerRef, teamSide);
        }

        private void SetCamera(PlayerRef playerRef, Player player)
        {
            CameraController cameraController = Runner.Spawn(_cameraControllerPrefab, player.transform.position,
                Quaternion.identity,
                playerRef);

            _playerServices[playerRef].Add(cameraController.Object);
        }

        private void SetInputService(PlayerRef playerRef)
        {
            InputService inputService = Runner.Spawn(_inputServicePrefab, Vector3.zero, Quaternion.identity, playerRef);

            _playerServices[playerRef].Add(inputService.Object);
        }

        public Vector3 GetPlayerPos(PlayerRef playerRef) => _players[playerRef].transform.position;

        public void DespawnPlayer(PlayerRef playerRef)
        {
            PlayerLeft(playerRef);
        }

        public void SetPlayerActiveState(PlayerRef playerRef, bool isOn)
        {
            Player player = _players[playerRef];

            player.gameObject.SetActive(isOn);

            player.PlayerController.AllowToMove = isOn;
            player.PlayerController.AllowToShoot = isOn;
        }

        public void PlayerLeft(PlayerRef playerRef)
        {
            DeSpawned.OnNext(playerRef);

            Player player = _players[playerRef];

            PlayerManager.RemovePlayer(player);
            
            Runner.Despawn(player.Object);

            foreach (NetworkObject networkObject in _playerServices[playerRef])
            {
                Runner.Despawn(networkObject);
            }

            _teamsService.RemoveFromTeam(playerRef);

            _playerServices.Remove(playerRef);

            _players.Remove(playerRef);

            
            
            PlayersCount--;
        }

        public void RespawnPlayer(PlayerRef playerRef)
        {
            _playersHealthService.RestorePlayerHealth(playerRef);

            TeamSide playerTeamSide = _teamsService.GetPlayerTeamSide(playerRef);

            var spawnPoints = LevelService.Instance.CurrentLevel.GetSpawnPointsByTeam(playerTeamSide);

            SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Player player = _players[playerRef];

            player.transform.position = spawnPoint.transform.position;

            player.PlayerController.AllowToMove = true;
            player.PlayerController.AllowToShoot = true;

            player.HitboxRoot.HitboxRootActive = true;

            ColorTeamBanner(playerRef);
        }

        public Vector3 GetSpawnPos(TeamSide teamSide)
        {
            var spawnPoints = LevelService.Instance.CurrentLevel.GetSpawnPointsByTeam(teamSide);

            SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            return spawnPoint.transform.position;
        }


        private void RPC_OnPlayerSpawnedInvoke(Player player)
        {
            var spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.PlayerRef = player.Object.InputAuthority;
            spawnEventContext.Transform = player.transform;
            spawnEventContext.CharacterClass = player.CharacterClass;

//            Debug.Log($"Player spawned");
            Spawned.OnNext(spawnEventContext);
        }

        public bool TryGetPlayer(PlayerRef playerRef, out Player player)
        {
            player = null;

            foreach (var keyValuePair in _players)
            {
                if (keyValuePair.Key == playerRef)
                {
                    player = keyValuePair.Value;
                    return true;
                }
            }


            return false;
        }
    }


    public struct PlayerSpawnEventContext
    {
        public CharacterClass CharacterClass;
        public PlayerRef PlayerRef;
        public Transform Transform;
    }
}