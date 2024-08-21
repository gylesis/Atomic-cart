using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure
{
    [RequireComponent(typeof(NetworkRunner))]
    public class LobbyConnector : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _networkRunner;

        public NetworkRunner NetworkRunner => _networkRunner;

        public static bool IsConnected;

        private Action _sessionJoined;
        private SceneLoader _sceneLoader;

        public static LobbyConnector Instance { get; private set; }
        
        [Inject]
        private void Construct(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }
        
        private async void Awake()
        {
            if (IsConnected)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            DiInjecter.Instance.InjectGameObject(gameObject);
            
            if (SceneManager.GetActiveScene().name == "Main")
            {
                Connect();
            }

            DontDestroyOnLoad(gameObject);
        }

        public void ConnectFromBootstrap()
        {
            LoadFromBootstrap();
        }

        public void Connect()
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

            string msg;
            
            if (gameResult.Ok)
            {
                msg = "Welcome!";
                Curtains.Instance.HideWithDelay(1, 0.5f);
                AtomicLogger.Log($"Joined lobby");
            }
            else
            {
                msg = "Failed to connect to servers";
                AtomicLogger.Err($"Failed to Start: {gameResult.ShutdownReason}, {gameResult.ErrorMessage}"); // TODO add button to reconnect to photon lobby 
            }
            
            Curtains.Instance.SetText(msg);
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
            Curtains.Instance.SetText("Game closed");
            Curtains.Instance.Show();
            await SceneManager.LoadSceneAsync(0);
            Curtains.Instance.Hide();
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