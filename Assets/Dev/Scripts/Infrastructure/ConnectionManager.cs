using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev.Levels;
using Dev.UI;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure
{
    public class ConnectionManager : NetworkContext, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner _networkRunnerPrefab;
        
        private PlayersSpawner _playersSpawner;
        private PopUpService _popUpService;

        public static ConnectionManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            
            if (FindObjectOfType<NetworkRunner>() != null)
            {
                return;
            }
            else
            {
                NetworkRunner runner = Instantiate(_networkRunnerPrefab);

                runner.AddCallbacks(this);

                var startGameArgs = new StartGameArgs();

                startGameArgs.GameMode = GameMode.Shared;
                startGameArgs.SceneManager = FindObjectOfType<SceneLoader>();
                startGameArgs.Scene = SceneManager.GetActiveScene().buildIndex;

                runner.StartGame(startGameArgs);
            }
        }

        [Inject]
        private void Init(PlayersSpawner playersSpawner, PopUpService popUpService)
        {
            _popUpService = popUpService;
            _playersSpawner = playersSpawner;
        }

        public override void Spawned()
        {
            Runner.AddCallbacks(this);
        }

        public void Disconnect()
        {
            Runner.Shutdown();

            PlayerManager.AllPlayers.Clear();
            PlayerManager.PlayerQueue.Clear();
            
            _popUpService.HideAllPopUps();

            SceneManager.LoadScene(0);
        }
        
        public async void OnPlayerJoined(NetworkRunner runner, PlayerRef player) // for new player after lobby started. invokes if game starts from Lobby
        {
            Debug.Log($"[ConnectionManager] Player joined {player}");
           
            if (runner.GameMode == GameMode.Shared)
            {
                Debug.Log($"Someone connected to the game");
                Debug.Log($"Spawning player... {player}");

                await Task.Delay(2000);
             
                _playersSpawner.SpawnPlayerByCharacterClass(runner, player);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"On Player Left");
            
            if (runner.IsSharedModeMasterClient)
            {
                Debug.Log($"Despawning player");
                _playersSpawner.DespawnPlayer(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"On Shutdown: {shutdownReason}");
        }

        public async void OnConnectedToServer(NetworkRunner runner) // invokes if game starts from Main scene
        {
            if (runner.GameMode == GameMode.Shared)
            {
                PlayerRef playerRef = runner.LocalPlayer;

                Debug.Log($"Someone connected to the game");
                Debug.Log($"Spawning player... {playerRef}");

                await Task.Delay(2000);
             
                _playersSpawner.SpawnPlayerByCharacterClass(runner, playerRef);
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.Log($"On disconnect from server");
            Application.Quit();
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.Log($"On Host migration");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public async void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log($"OnSceneLoadDone");

            LevelService.Instance.LoadLevel(GameStaticData.LevelName);
            
            await Task.Delay(3000); // TODO 

            foreach (PlayerRef playerRef in PlayerManager.PlayerQueue)
            {
                // Debug.Log($"Respawning Player {_playersDataService.GetNickname(playerRef)}");
                Debug.Log($"Spawning Player {playerRef}");

                _playersSpawner.SpawnPlayerByCharacterClass(runner,playerRef);
            }

            PlayerManager.PlayerQueue.Clear();
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log($"OnSceneLoadStart");
        }
    }
}