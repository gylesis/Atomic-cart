using System.Collections.Generic;
using Fusion;
using UniRx;
using Unity.Mathematics;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class PlayersSpawner : NetworkContext
    {
        [SerializeField] private Joystick _movementJoystick;
        [SerializeField] private Joystick _aimJoystick;
        
        [SerializeField] private InputService _inputServicePrefab;
        [SerializeField] private NetworkObject _playerPrefab;

        public Joystick MovementJoystick => _movementJoystick;
        public Joystick AimJoystick => _aimJoystick;

        [Networked]
        private int PlayersCount { get; set; }

        private Dictionary<PlayerRef, Player> _players = new Dictionary<PlayerRef, Player>();

        public Subject<PlayerSpawnEventContext> Spawned { get; } = new Subject<PlayerSpawnEventContext>();

        [Networked]
        private NetworkDictionary<PlayerRef, InputService> _inputs { get; }

        public Player SpawnPlayer(PlayerRef playerRef)
        {
            var playersLength = PlayersCount;
            
            var playerNetObj = Runner.Spawn(_playerPrefab, Vector2.zero + Vector2.right * playersLength, quaternion.identity, playerRef);
            var player = playerNetObj.GetComponent<Player>();

            InputService inputService = Runner.Spawn(_inputServicePrefab, Vector3.zero, Quaternion.identity, playerRef);

            _inputs.Add(playerRef, inputService);

            Runner.SetPlayerObject(playerRef, playerNetObj);

            var playerName = $"Player №{playerNetObj.InputAuthority.PlayerId}";
                
            player.RPC_SetName(playerName);
            
            PlayersCount++;

            RPC_OnPlayerSpawnedInvoke(player);
            
            _players.Add(playerRef,player);

            return player;
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
        
        public void PlayerLeft(PlayerRef playerRef)
        {
            Player player = _players[playerRef];
            
            Runner.Despawn(player.Object);

            _players.Remove(playerRef);

            PlayersCount--;
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