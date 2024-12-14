

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dev.Levels;
using Dev.Sounds;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure.Networking
{
    [RequireComponent(typeof(NetworkRunner))]
    public class ConnectionManager : MonoSingleton<ConnectionManager>, INetworkRunnerCallbacks
    {
        public NetworkRunner NetworkRunner
        {
            get
            {
                if(_networkRunner == null)
                    _networkRunner = GetComponent<NetworkRunner>();
                
                return _networkRunner;
            }
        }

        public bool IsConnected { get; set; }
        private bool IsOnMainScene => SceneManager.GetActiveScene().name == "Main";

        public bool IsHostedProperly { get; private set; }
        
        private Action _sessionJoined;
        private NetworkRunner _networkRunner;

        private SceneLoader _sceneLoader;
        private PopUpService _popUpService;
        private ModulesService _modulesService;

        [Inject]
        private void Construct(SceneLoader sceneLoader, PopUpService popUpService, ModulesService modulesService) // probably buggy
        {
            _modulesService = modulesService;
            _popUpService = popUpService;
            _sceneLoader = sceneLoader;
        }
        
        protected override void Awake()
        {
            if (IsHostedProperly)
            {
                Destroy(gameObject);
                return;
            }

            base.Awake();

            DiInjecter.Instance.InjectGameObject(gameObject);
            
            if (IsOnMainScene) 
                ConnectFromMain();

            DontDestroyOnLoad(gameObject);
        }

        public void ConnectFromBootstrap()
        {
            LoadFromBootstrap();
        }

        public async void ConnectFromMain()
        {
            Curtains.Instance.SetText("Starting test session");
            Curtains.Instance.ShowWithDotAnimation();
            
            NetworkRunner.ProvideInput = true;
            NetworkRunner.AddCallbacks(this);
            
            var startGameArgs = new StartGameArgs();

            startGameArgs.GameMode = GameMode.Shared;
            startGameArgs.SceneManager = NetworkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            startGameArgs.Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            startGameArgs.SessionName = "Test1";

            var gameResult = await NetworkRunner.StartGame(startGameArgs);
    
            string msg = gameResult.Ok ? "Welcome!" : "Failed to connect to servers";
            Curtains.Instance.SetText(msg);

            IsConnected = gameResult.Ok;
            
            if (gameResult.Ok)
            {
                Curtains.Instance.HideWithDelay(1, 0.5f);
                AtomicLogger.Log($"Joined lobby", AtomicConstants.LogTags.Networking);
            }
            else
            {
                AtomicLogger.Err($"Failed to Start: {gameResult.ShutdownReason}, {gameResult.ErrorMessage}", AtomicConstants.LogTags.Networking); // TODO add button to reconnect to photon lobby 
               // _sceneLoader.LoadSceneLocal(0);
            }
        }

        private async void DefaultJoinToSessionLobby()
        {
            SoundController.Instance.PlayMainMusic(true);
            
            Curtains.Instance.SetText("Joining to servers");
            Curtains.Instance.ShowWithDotAnimation();

            NetworkRunner.ProvideInput = true;
            NetworkRunner.AddCallbacks(this);
            
            StartGameResult gameResult = await NetworkRunner.JoinSessionLobby(SessionLobby.Shared, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            OnLobbyJoined(gameResult);

            string msg = gameResult.Ok ? "Welcome!" : "Failed to connect to servers";
            Curtains.Instance.SetText(msg);

            IsConnected = gameResult.Ok;
            
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
            IsHostedProperly = true;

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
        
        public void Disconnect()
        {
            Curtains.Instance.Show();
            Curtains.Instance.SetText("Returning back to menu");
            
            IsConnected = false;
            IsHostedProperly = false;
            
            NetworkRunner.Shutdown();

            PlayerManager.PlayersOnServer.Clear();
            PlayerManager.LoadingPlayers.Clear();

            if (_popUpService != null) 
                _popUpService.HideAllPopUps();

            SceneManager.LoadScene(0);
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

        public async void OnSceneLoadDone(NetworkRunner runner)
        {
            AtomicLogger.Log("Scene load done");
            
            if (IsOnMainScene)
            {
                if (!_modulesService.IsInitialized) // loaded from MainScene
                {
                    var initializeResult = await _modulesService.Initialize();

                    if (initializeResult.IsError)
                    {
                        Curtains.Instance.SetText("Error initializing services");
                        AtomicLogger.Err("Failed to initialize modules", initializeResult.ErrorMessage);
                        return;
                    }
                    
                }
                
                if (runner.IsSharedModeMasterClient)
                {
                    if (IsHostedProperly)
                    {
                        LevelService.Instance.LoadLevel(NetworkRunner.SessionInfo.Properties["map"]);
                        await UniTask.Delay(2000); // TODO wait until all players load the scene
                    }
                    else
                    {
                        LevelService.Instance.LoadLevel(GameSettingsProvider.GameSettings.DebugMap.ToString());
                    }
                }

                PlayerRef player = runner.LocalPlayer;
                PlayersSpawner.Instance.AskCharacterAndSpawn(player);
                PlayerManager.LoadingPlayers.Remove(player);
            }
            else
            {
                Curtains.Instance.SetText("Finishing");
                Curtains.Instance.HideWithDelay(1);
            }
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

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (IsOnMainScene)
            {
                AtomicLogger.Log($"Someone's late connection to the game {player}");
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (IsOnMainScene)
            {
                AtomicLogger.Log($"On Player Left");
                if (runner.IsSharedModeMasterClient)
                {
                    AtomicLogger.Log($"Despawning player");
                    PlayersSpawner.Instance.DespawnPlayer(player, true);
                }
            }
        }

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