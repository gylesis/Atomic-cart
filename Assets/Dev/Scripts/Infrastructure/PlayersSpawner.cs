using System;
using System.Collections.Generic;
using System.Linq;
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

        public Subject<PlayerSpawnEventContext> Spawned { get; } = new Subject<PlayerSpawnEventContext>();
        public Subject<PlayerRef> DeSpawned { get; } = new Subject<PlayerRef>();

        private Dictionary<PlayerRef, List<NetworkObject>> _playerServices =
            new Dictionary<PlayerRef, List<NetworkObject>>();

  

        [Networked, Capacity(10)] public NetworkLinkedList<Player> Players { get; }

//        public IReadOnlyCollection<Player> Players => _players.Values;
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

        public void SpawnPlayerByCharacterClass(NetworkRunner runner, PlayerRef playerRef)
        {
            GetCharacterClassAndSpawn(playerRef, runner);
        }

        private void GetCharacterClassAndSpawn(PlayerRef playerRef, NetworkRunner networkRunner)
        {   
            _popUpService.TryGetPopUp<CharacterChooseMenu>(out var characterChooseMenu);

            characterChooseMenu.StartChoosingCharacter((characterClass =>
            {
                Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe((l =>
                {
                    _popUpService.HidePopUp<CharacterChooseMenu>();
                    _popUpService.ShowPopUp<HUDMenu>();
                }));

                SpawnPlayerByCharacter(characterClass, playerRef, networkRunner);
            }));
        }

        private void SpawnPlayerByCharacter(CharacterClass characterClass, PlayerRef playerRef,
            NetworkRunner networkRunner)
        {
            Debug.Log($"Player {playerRef} chose {characterClass}");

            SpawnPlayer(playerRef, characterClass, networkRunner);
        }

        public Player SpawnPlayer(PlayerRef playerRef, CharacterClass characterClass, NetworkRunner networkRunner)
        {
            AssignTeam(playerRef);

            CharacterData characterData = _charactersDataContainer.GetCharacterDataByClass(characterClass);

            Player playerPrefab = characterData.PlayerPrefab;

            TeamSide teamSide = _teamsService.GetPlayerTeamSide(playerRef);
            Vector3 spawnPos = GetSpawnPos(teamSide);

            Player player = networkRunner.Spawn(playerPrefab, spawnPos,
                quaternion.identity, playerRef);
            
            NetworkObject playerNetObj = player.Object;

            playerNetObj.RequestStateAuthority();
            playerNetObj.AssignInputAuthority(playerRef);
            networkRunner.SetPlayerObject(playerRef, playerNetObj);

            RPC_AddPlayer(player);
                
            PlayerManager.AddPlayer(player);
            
            player.PlayerController.RPC_Init(characterData.CharacterStats.MoveSpeed,
                characterData.CharacterStats.ShootThreshold, characterData.CharacterStats.SpeedLowerSpeed);

            player.PlayerController.AllowToMove = true;
            player.PlayerController.AllowToShoot = true;
            
            player.Init(characterClass);

            _playerServices.Add(playerRef, new List<NetworkObject>());

            SetInputService(playerRef, networkRunner);
            SetCamera(playerRef, player, networkRunner);

            var playerName = $"Player №{playerNetObj.InputAuthority.PlayerId}";
            player.RPC_SetName(playerName);

            PlayersCount++;

            //RespawnPlayer(playerRef);

            RPC_OnPlayerSpawnedInvoke(player);

            LoadWeapon(player);

            ColorTeamBanner(playerRef);

            return player;
        }

        [Rpc]
        private void RPC_AddPlayer(Player player)
        {
            Players.Add(player);
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

        public Player GetPlayer(PlayerRef playerRef)
        {
            return Players.First(x => x.Id == playerRef);
        }
        
        
        private void LoadWeapon(Player player)
        {
            //  var weaponSetupContext = new WeaponSetupContext("AkWeapon");
            //  player.WeaponController.Init(weaponSetupContext);
        }

        private void AssignTeam(PlayerRef playerRef)
        {
            TeamSide teamSide = TeamSide.Blue;

            if (PlayersCount % 2 == 0)
            {
                teamSide = TeamSide.Red;
            }

            _teamsService.AssignForTeam(playerRef, teamSide);
        }

        private void SetCamera(PlayerRef playerRef, Player player, NetworkRunner networkRunner)
        {
            CameraController cameraController = networkRunner.Spawn(_cameraControllerPrefab, player.transform.position,
                Quaternion.identity,
                playerRef);

            _playerServices[playerRef].Add(cameraController.Object);
        }

        private void SetInputService(PlayerRef playerRef, NetworkRunner networkRunner)
        {
            InputService inputService = networkRunner.Spawn(_inputServicePrefab, Vector3.zero, Quaternion.identity, playerRef);

            _playerServices[playerRef].Add(inputService.Object);
        }

        public Vector3 GetPlayerPos(PlayerRef playerRef) => GetPlayer(playerRef).transform.position;

        public void DespawnPlayer(PlayerRef playerRef)
        {
            PlayerLeft(playerRef);
        }

        public void SetPlayerActiveState(PlayerRef playerRef, bool isOn)
        {
            Player player = GetPlayer(playerRef);

            player.gameObject.SetActive(isOn);

            player.PlayerController.AllowToMove = isOn;
            player.PlayerController.AllowToShoot = isOn;
        }

        public void PlayerLeft(PlayerRef playerRef)
        {
            Player player = GetPlayer(playerRef);
            
            Runner.Despawn(player.Object);
            
            DeSpawned.OnNext(playerRef);
            
            PlayerManager.RemovePlayer(player);
            
            _playerServices.Remove(playerRef);
            
            _teamsService.RemoveFromTeam(playerRef);
            Players.Remove(player);
            PlayersCount--;
        }

        public void RespawnPlayer(PlayerRef playerRef)
        {
            _playersHealthService.RestorePlayerHealth(playerRef);

            TeamSide playerTeamSide = _teamsService.GetPlayerTeamSide(playerRef);

            var spawnPoints = LevelService.Instance.CurrentLevel.GetSpawnPointsByTeam(playerTeamSide);

            SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Player player = GetPlayer(playerRef);

            player.RPC_SetPos(spawnPoint.transform.position);

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

        [Rpc]
        private void RPC_OnPlayerSpawnedInvoke(Player player)
        {
            var spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.PlayerRef = player.Object.InputAuthority;
            spawnEventContext.Transform = player.transform;
            spawnEventContext.CharacterClass = player.CharacterClass;

            Debug.Log($"[RPC] Player spawned");
            Spawned.OnNext(spawnEventContext);
        }

    }

    public struct PlayerSpawnEventContext
    {
        public CharacterClass CharacterClass;
        public PlayerRef PlayerRef;
        public Transform Transform;
    }
    
}