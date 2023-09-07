using System;
using System.Collections.Generic;
using System.Linq;
using Dev.UI;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev.Infrastructure
{
    public class GameSessionBrowser : NetworkContext, INetworkRunnerCallbacks
    {
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

            startGameArgs.GameMode = GameMode.Host;
            startGameArgs.SessionName = _inputField.text;
            startGameArgs.SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            StartGameResult startGameResult = await _runner.StartGame(startGameArgs);

            if (startGameResult.Ok)
            {
                FindObjectOfType<SceneLoader>().LoadScene("Main");
            }
           
        }

        [ContextMenu(nameof(JoinSession))]
        public void JoinSession()
        {
            var startGameArgs = new StartGameArgs();

            startGameArgs.GameMode = GameMode.Client;
            startGameArgs.SessionName = _inputField.text;

            _runner.StartGame(startGameArgs);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

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