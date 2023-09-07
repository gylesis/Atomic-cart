using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev.UI;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Dev.Infrastructure
{
    public class GameSessionBrowser : NetworkContext, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkObject _playerPrefab;
        
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private DefaultReactiveButton _joinButton;
        [SerializeField] private DefaultReactiveButton _hostButton;
        [SerializeField] private TMP_Text _debugText;   
            
        private List<SessionInfo> _sessionInfos;

        private NetworkRunner _runner;

        private void Awake()
        {
            _runner = FindObjectOfType<NetworkRunner>();

            _hostButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnHostButtonClicked()));
            _joinButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnJoinButtonClicked()));
        }

        private void OnHostButtonClicked()
        {
            CreateSession();
        }
        
        private void OnJoinButtonClicked()
        {
            JoinSession();
        }

        [ContextMenu(nameof(CreateSession))]
        public async void CreateSession()
        {
            var startGameArgs = new StartGameArgs();

            _runner.AddCallbacks(this);
            
            startGameArgs.GameMode = GameMode.Host;
            startGameArgs.SessionName = _inputField.text;
            startGameArgs.SceneManager = FindObjectOfType<LevelManager>();
            startGameArgs.Scene = SceneManager.GetActiveScene().buildIndex;

            StartGameResult startGameResult = await _runner.StartGame(startGameArgs);
        }

        [ContextMenu(nameof(JoinSession))]
        public void JoinSession()
        {
            var startGameArgs = new StartGameArgs();

            startGameArgs.GameMode = GameMode.Client;
            startGameArgs.SessionName = _inputField.text;
            startGameArgs.SceneManager = FindObjectOfType<LevelManager>();

            _runner.StartGame(startGameArgs);
        }

        public async void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log($"Player joined {playerRef}");
            
            if (runner.IsServer)
            {
                PlayerManager.AddPlayerForQueue(playerRef);
                
                Vector3 spawnPos = Vector3.zero + Vector3.right * Random.Range(-10f,10f);

                var player = _runner.Spawn(_playerPrefab, spawnPos,
                    quaternion.identity, playerRef);

                if (PlayerManager.PlayerQueue.Count > 1)
                {
                    await Task.Delay(500);
                    
                     FindObjectOfType<SceneLoader>().LoadScene("Main");
                     
                    
                }
            }
            
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _sessionInfos = sessionList;

            _debugText.text = String.Empty;
            
            foreach (SessionInfo sessionInfo in sessionList)
            {
                string message = $"Session {sessionInfo.Name}, Region {sessionInfo.Region}, Players {sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}, IsOpen {sessionInfo.IsOpen}";
               
                Debug.Log(message);
                _debugText.text += message + "\n";
            }
    
            if (sessionList.Count > 0)
            {
                _inputField.text = sessionList.Last().Name;
            }
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}