using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev.Infrastructure
{
    [RequireComponent(typeof(NetworkRunner))]
    public class FusionLobbyConnector : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _networkRunner;

        public NetworkRunner NetworkRunner => _networkRunner;

        public static bool IsConnected;

        private Action _sessionJoined;
            
        private void Awake()
        {
            if (IsConnected)
            {
                Destroy(gameObject);
                return;
            }
            
            IsConnected = true;
            DontDestroyOnLoad(gameObject);

            if (SceneManager.GetActiveScene().name == "Bootstrap")
            {
                LoadFromBootstrap();
            }
            else
            {
                DefaultJoinToSessionLobby();
            }
        }

        private async void DefaultJoinToSessionLobby()
        {
            _networkRunner = GetComponent<NetworkRunner>();
            _networkRunner.ProvideInput = true;
            
            var gameResult = await _networkRunner.JoinSessionLobby(SessionLobby.Shared);

            OnLobbyJoined(gameResult);
            
            if (gameResult.Ok)
            {
                Debug.Log($"Joined lobby");
            }
            else
            {
                Debug.LogError($"Failed to Start: {gameResult.ShutdownReason}");
            }
        }

        private async void LoadFromBootstrap()
        {
            Curtains.Instance.Show();
            Curtains.Instance.SetText("Joining to servers");
            
            Scene activeScene = SceneManager.GetActiveScene();

            DefaultJoinToSessionLobby();
            
            await SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Additive);
            await SceneManager.UnloadSceneAsync(activeScene);
            
            AddLobbyJoinCallback((() =>
            {
                Curtains.Instance.SetText("Done!");
                Curtains.Instance.HideWithDelay(1);
            }));
        }

        private void AddLobbyJoinCallback(Action onSessionJoin)
        {
            _sessionJoined += onSessionJoin;
        }
    
        private void OnLobbyJoined(StartGameResult gameResult)
        {
            _sessionJoined?.Invoke();
            _sessionJoined = null;
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}