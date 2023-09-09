using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure
{
    public class EntryPoint : NetworkContext, INetworkRunnerCallbacks
    {
        private PlayersSpawner _playersSpawner;

        private void Awake()
        {
            if (FindObjectOfType<NetworkRunner>() != null)
            {
                return;
            }
            else
            {
                NetworkRunner runner = gameObject.AddComponent<NetworkRunner>();

                runner.AddCallbacks(this);
                
                var startGameArgs = new StartGameArgs();

                startGameArgs.GameMode = GameMode.Host;
                startGameArgs.SceneManager = FindObjectOfType<LevelManager>();
                startGameArgs.Scene = SceneManager.GetActiveScene().buildIndex;

                runner.StartGame(startGameArgs);
            }
        }

        [Inject]
        private void Init(PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
        }

        public override void Spawned()
        {
            Runner.AddCallbacks(this);
        }

        public async void OnPlayerJoined(NetworkRunner runner, PlayerRef player) // for new player after lobby started
        {
            Debug.Log($"Player Joined");

            if (runner.IsServer)
            {
                await Task.Delay(500);

                _playersSpawner.SpawnPlayerByCharacterClass(player);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                _playersSpawner.DespawnPlayer(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public async void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log($"OnSceneLoadDone");

            await Task.Delay(3000); // TODO 

            foreach (PlayerRef playerRef in PlayerManager.PlayerQueue)
            {
                // Debug.Log($"Respawning Player {_playersDataService.GetNickname(playerRef)}");
                Debug.Log($"Spawning Player {playerRef}");

                _playersSpawner.SpawnPlayerByCharacterClass(playerRef);
            }

            PlayerManager.PlayerQueue.Clear();
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log($"OnSceneLoadStart");
        }
    }
}