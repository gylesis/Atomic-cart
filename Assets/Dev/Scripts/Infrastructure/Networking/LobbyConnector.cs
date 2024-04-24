using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev.Infrastructure
{
    [RequireComponent(typeof(NetworkRunner))]
    public class LobbyConnector : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _networkRunner;

        public NetworkRunner NetworkRunner => _networkRunner;

        public static bool IsConnected;

        private Action _sessionJoined;

        [SerializeField] private bool _hasInternet;
        
        private async void Awake()
        {
            if (IsConnected)
            {
                Destroy(gameObject);
                return;
            }
           
            DontDestroyOnLoad(gameObject);

            InternetCheckProcedure();
        }

        private async void InternetCheckProcedure()
        {   
            bool hasInternetConnection = await CheckInternetConnection();
    
            if (hasInternetConnection)
            {
                IsConnected = true;
                
                if (SceneManager.GetActiveScene().name == "Bootstrap")
                {
                    LoadFromBootstrap();
                }
                else
                {
                    DefaultJoinToSessionLobby();
                }
            }
            else
            {
                await UniTask.Delay(1000);
                
                InternetCheckProcedure();
            }
            
        }

        private async UniTask<bool> CheckInternetConnection()
        {
            Curtains.Instance.Show();
            Curtains.Instance.SetText("Checking internet connection...");

            await UniTask.Delay(200);

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Curtains.Instance.SetText("No internet connection! Please configure internet connection!");
                
                await UniTask.Delay(500);
                return false;
            }
            
            await UniTask.Delay(500);

            return true;
        }
        
        private async void DefaultJoinToSessionLobby()
        {
            _networkRunner = GetComponent<NetworkRunner>();
            _networkRunner.ProvideInput = true;

            var authenticationValues = new AuthenticationValues();
            authenticationValues.AddAuthParameter("hi", "228");
            
            var gameResult = await _networkRunner.JoinSessionLobby(SessionLobby.Shared, authentication: authenticationValues );
            
            OnLobbyJoined(gameResult);
            
            if (gameResult.Ok)
            {
                Debug.Log($"Joined lobby");
            }
            else
            {
                Debug.LogError($"Failed to Start: {gameResult.ShutdownReason}, {gameResult.ErrorMessage}");
                int a  = 2;
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

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            Debug.Log($"Custom auth response");
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Curtains.Instance.SetText("Loading world...");
            Curtains.Instance.Show();
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Curtains.Instance.SetText("Finishing");
            Curtains.Instance.HideWithDelay(1);
        }
    }
}