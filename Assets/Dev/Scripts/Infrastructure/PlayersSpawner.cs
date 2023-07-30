using System;
using System.Collections.Generic;
using Fusion;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.Infrastructure
{
    public class PlayersSpawner : NetworkContext
    {
        [SerializeField] private Joystick _movementJoystick;
        [SerializeField] private Joystick _aimJoystick;

        [SerializeField] private InputService _inputServicePrefab;
        [SerializeField] private NetworkObject _playerPrefab;

        [SerializeField] private CameraController _cameraControllerPrefab;

        public Joystick MovementJoystick => _movementJoystick;
        public Joystick AimJoystick => _aimJoystick;

        [Networked] public int PlayersCount { get; set; }

        private Dictionary<PlayerRef, Player> _players = new Dictionary<PlayerRef, Player>();

        public Subject<PlayerSpawnEventContext> Spawned { get; } = new Subject<PlayerSpawnEventContext>();
        public Subject<PlayerRef> DeSpawned { get; } = new Subject<PlayerRef>();

        private Dictionary<PlayerRef, List<NetworkObject>> _playerServices =
            new Dictionary<PlayerRef, List<NetworkObject>>();

        private TeamsService _teamsService;

        private void Awake()
        {
            _teamsService = FindObjectOfType<TeamsService>(); // TODO TEMP, need DI
        }

        public Player SpawnPlayer(PlayerRef playerRef)
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

            RPC_OnPlayerSpawnedInvoke(player);

            AssignTeam(playerRef);

            return player;
        }

        private void AssignTeam(PlayerRef playerRef)
        {
            TeamSide teamSide = TeamSide.Red;

            Color color = Color.red;
            
            if (_players.Count % 2 == 0)
            {
                teamSide = TeamSide.Blue;
                color = Color.blue;
            }

            _teamsService.AssignForTeam(playerRef, teamSide);
            
            _players[playerRef].PlayerView.RPC_SetTeamColor(color);
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

        public void DespawnPlayer(PlayerRef playerRef)
        {
            PlayerLeft(playerRef);
        }

        public void PlayerLeft(PlayerRef playerRef)
        {
            DeSpawned.OnNext(playerRef);

            Player player = _players[playerRef];

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
            var spawnPoints = LevelService.Instance.CurrentLevel.SpawnPoints;

            SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Player player = _players[playerRef];

            player.transform.position = spawnPoint.transform.position;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_OnPlayerSpawnedInvoke(Player player)
        {
            var spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.PlayerRef = player.Object.InputAuthority;
            spawnEventContext.Transform = player.transform;

            Debug.Log($"Player spawned");
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
        public PlayerRef PlayerRef;
        public Transform Transform;
    }
}