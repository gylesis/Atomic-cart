using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev.Levels;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure
{
    public class MainSceneConnectionManager : NetworkContext, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner _networkRunner;

        private PlayersSpawner _playersSpawner;
        private PopUpService _popUpService;
        private GameSettings _gameSettings;

        public static MainSceneConnectionManager Instance { get; private set; }

        protected override void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            if (LobbyConnector.IsConnected)
            {
                LobbyConnector.IsConnected = false;
                NetworkRunner networkRunner = FindObjectOfType<LobbyConnector>().NetworkRunner;
                
                networkRunner.AddCallbacks(this);

                if (networkRunner.IsConnectedToServer)
                {
                    _networkRunner.gameObject.SetActive(false);
                }
            }
            else
            {
                _networkRunner.AddCallbacks(this);

                var startGameArgs = new StartGameArgs();

                startGameArgs.GameMode = GameMode.Shared;
                startGameArgs.SceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
                startGameArgs.Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
                startGameArgs.SessionName = "Test1";

                _networkRunner.StartGame(startGameArgs);
            }
        }

        [Inject]
        private void Init(PlayersSpawner playersSpawner, PopUpService popUpService, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _popUpService = popUpService;
            _playersSpawner = playersSpawner;
        }

        public void Disconnect()
        {
            Curtains.Instance.Show();
            Curtains.Instance.SetText("Returning back to menu");
            
            LobbyConnector.IsConnected = false;
            
            Runner.Shutdown();

            PlayerManager.PlayersOnServer.Clear();
            PlayerManager.LoadingPlayers.Clear();

            if (_popUpService != null)
            {
                _popUpService.HideAllPopUps();
            }

            SceneManager.LoadScene(0);
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        // for new player after lobby started. invokes if game starts from Lobby
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
        {
            AtomicLogger.Log($"Someone's late connection to the game {player}");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            AtomicLogger.Log($"On Player Left");

            if (runner.IsSharedModeMasterClient)
            {
                AtomicLogger.Log($"Despawning player");
                _playersSpawner.DespawnPlayer(player, true);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            AtomicLogger.Log($"On Shutdown: {shutdownReason}");
        }

        public async void OnConnectedToServer(NetworkRunner runner) // invokes if game starts from Main scene
        {
            PlayerRef playerRef = runner.LocalPlayer;

            AtomicLogger.Log($"Someone connected to the game, Spawning player... {playerRef}");

            await Task.Delay(2000);

            //_playersSpawner.ChooseCharacterClass(playerRef);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
                                     byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            AtomicLogger.Log($"On Host migration");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
                                           ArraySegment<byte> data) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public async void OnSceneLoadDone(NetworkRunner runner)
        {
            AtomicLogger.Log($"OnSceneLoadDone");

            if (runner.IsSharedModeMasterClient)
            {
                if (_gameSettings.IsDebugMode)
                {
                    await SaveLoadService.Instance.Load();
                    LevelService.Instance.LoadLevel("NightCity");
                }
                else
                {
                     LevelService.Instance.LoadLevel(LobbyConnector.Instance.NetworkRunner.SessionInfo.Properties["map"]);
                     await Task.Delay(2000); // TODO wait until all players load the scene
                }
                
            }
            
            PlayerRef player = runner.LocalPlayer;
            _playersSpawner.AskCharacterAndSpawn(player);
            PlayerManager.LoadingPlayers.Remove(player);
        }

        [Rpc]
        private void RPC_OnSceneLoaded(PlayerRef playerRef)
        {
            PlayerManager.LoadingPlayers.Remove(playerRef);
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log($"OnSceneLoadStart");
        }
    }
}