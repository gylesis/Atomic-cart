using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dev.UI;
using Dev.Utils;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure.Networking
{
    [RequireComponent(typeof(NetworkRunner))]
    public class LobbyConnector : MonoSingleton<LobbyConnector>, INetworkRunnerCallbacks
    {
        public NetworkRunner NetworkRunner => _networkRunner;
        public bool IsConnected { get; set; }

        private Action _sessionJoined;
        private SceneLoader _sceneLoader;
        private NetworkRunner _networkRunner;

        [Inject]
        private void Construct(SceneLoader sceneLoader) // probably buggy
        {
            _sceneLoader = sceneLoader;
        }
        
        protected override void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            base.Awake();
            
            if (IsConnected)
            {
                Destroy(gameObject);
                return;
            }

            DiInjecter.Instance.InjectGameObject(gameObject);
            
            if (SceneManager.GetActiveScene().name == "Main") 
                ConnectFromMain();

            DontDestroyOnLoad(gameObject);
        }

        public void ConnectFromBootstrap()
        {
            LoadFromBootstrap();
        }

        public void ConnectFromMain()
        {
            DefaultJoinToSessionLobby();
        }

        private async void DefaultJoinToSessionLobby()
        {
            Curtains.Instance.SetText("Joining to servers");
            Curtains.Instance.ShowWithDotAnimation();

            IsConnected = true;

            _networkRunner = GetComponent<NetworkRunner>();
            _networkRunner.ProvideInput = true;

            StartGameResult gameResult = await _networkRunner.JoinSessionLobby(SessionLobby.Shared, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            OnLobbyJoined(gameResult);

            string msg = gameResult.Ok ? "Welcome!" : "Failed to connect to servers";
            Curtains.Instance.SetText(msg);

            if (gameResult.Ok)
            {
                Curtains.Instance.HideWithDelay(1, 0.5f);
                AtomicLogger.Log($"Joined lobby", AtomicConstants.LogTags.Networking);
            }
            else
            {
                AtomicLogger.Err($"Failed to Start: {gameResult.ShutdownReason}, {gameResult.ErrorMessage}", AtomicConstants.LogTags.Networking); // TODO add button to reconnect to photon lobby 
                _sceneLoader.LoadSceneLocal(0);
            }
        }

        private async void LoadFromBootstrap()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            DefaultJoinToSessionLobby();

            await _sceneLoader.LoadSceneLocal("Lobby", LoadSceneMode.Additive)
                .AttachExternalCancellation(gameObject.GetCancellationTokenOnDestroy());
            await _sceneLoader.UnloadSceneLocal(activeScene.name)
                .AttachExternalCancellation(gameObject.GetCancellationTokenOnDestroy());
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

        public async void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
          //  Debug.Log($"Shutdown");
           // Curtains.Instance.SetText("Game closed");
           // Curtains.Instance.Show();
           // await SceneManager.LoadSceneAsync(0);
          //  Curtains.Instance.Hide();
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            AtomicLogger.Log("Scene load start");

            Curtains.Instance.SetText("Loading world...");
            Curtains.Instance.Show();
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            AtomicLogger.Log("Scene load done");
            Curtains.Instance.SetText("Finishing");
            Curtains.Instance.HideWithDelay(1);
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            AtomicLogger.Log($"Custom auth response");
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            AtomicLogger.Log($"Host migration!!");
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
                                     byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
                                           ArraySegment<byte> data) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    }
}