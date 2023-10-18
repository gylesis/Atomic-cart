using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev.UI;
using Fusion;
using Fusion.Sockets;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure
{
    public class GameSessionBrowser : NetworkContext, INetworkRunnerCallbacks
    {
        [SerializeField] private SessionUIView _sessionUIViewPrefab;

        [SerializeField] private UIElementsGroup _uiElementsGroup;

        private NetworkRunner _runner;
        private PopUpService _popUpService;
        private ObjectPool<SessionUIView> _sessionUIViewPool;
        
        public int PickedSessionId { get; private set; }

        private List<SessionGameInfo> _sessionGameInfos = new List<SessionGameInfo>(8);

        public Subject<int> SessionCountChanged { get; } = new Subject<int>();

        private List<SessionUIView> _sessionUIViews = new List<SessionUIView>(8);

        private void OnGUI()
        {
            string label = String.Empty;
            Color color = Color.white;

            if (_runner.LobbyInfo.IsValid)
            {
                label = "Connected";
                color = Color.green;
            }
            else
            {
                label = "Connecting...";
                color = Color.red;
            }

            var guiStyle = new GUIStyle();
            guiStyle.fontSize = 25;
            guiStyle.normal.textColor = color;

            var position = new Rect(Screen.width - 300, Screen.height - 150, 10, 10);

            GUI.Label(position, label, guiStyle);
        }

        private void Awake()
        {
            _sessionUIViewPool =
                new ObjectPool<SessionUIView>(CreateFunc, ActionOnGet, ActionOnRelease);
        }

        private void ActionOnRelease(SessionUIView sessionUIView)
        {
            sessionUIView.transform.parent = null;
            sessionUIView.SetEnableState(false);
        }

        private void ActionOnGet(SessionUIView sessionUIView)
        {
            sessionUIView.transform.parent = _uiElementsGroup.Parent;
            sessionUIView.SetEnableState(true);
        }

        private SessionUIView CreateFunc()
        {
            SessionUIView sessionUIView = Instantiate(_sessionUIViewPrefab, _uiElementsGroup.Parent);

            sessionUIView.Clicked.TakeUntilDestroy(this).Subscribe((OnSessionUIClicked));

            return sessionUIView;
        }

        private void OnSessionUIClicked(SessionUIView sessionUIView)
        {
            _uiElementsGroup.Select(sessionUIView);
        }

        [Inject]
        private void Init(PopUpService popUpService, NetworkRunner runner)
        {
            _runner = runner;
            _popUpService = popUpService;
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            int sessionsCount = sessionList.Count;   
            
            if (sessionsCount == 0)
            {
                PickedSessionId = -1;
            }
           
            SessionCountChanged.OnNext(sessionsCount);
         
            for (var index = _uiElementsGroup.UIElements.Count - 1; index >= 0; index--)
            {
                UIElementBase uiElementBase = _uiElementsGroup.UIElements[index];

                _sessionUIViewPool.Release(uiElementBase as SessionUIView);
                _uiElementsGroup.RemoveElement(uiElementBase);
            }

            _sessionGameInfos.Clear();
            _sessionUIViews.Clear();
            
            for (var index = 0; index < sessionsCount; index++)
            {
                SessionInfo sessionInfo = sessionList[index];
                var sessionGameInfo = new SessionGameInfo();

                sessionGameInfo.SessionName = sessionInfo.Name;
                sessionGameInfo.CurrentPlayers = sessionInfo.PlayerCount;
                sessionGameInfo.MaxPlayers = sessionInfo.MaxPlayers;
                sessionGameInfo.MapType = (MapType)sessionInfo.Properties["mode"].PropertyValue;
                sessionGameInfo.MapName = sessionInfo.Properties["map"].PropertyValue.ToString();
                sessionGameInfo.Id = index;

                _sessionGameInfos.Add(sessionGameInfo);
                
                SessionUIView sessionUIView = _sessionUIViewPool.Get();
                _uiElementsGroup.AddElement(sessionUIView);

                sessionUIView.Clicked.TakeUntilDestroy(this).Subscribe((OnSessionClicked));
                sessionUIView.UpdateInfo(sessionGameInfo);

                _sessionUIViews.Add(sessionUIView);
            }

            PickedSessionId = 0;
            if (_sessionUIViews.Count > 0)
            {
                _uiElementsGroup.Select(_sessionUIViews[PickedSessionId]);
            }
        }

        private void OnSessionClicked(SessionUIView obj)
        {
            PickedSessionId = obj.Id;
        }

        public async void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log($"Player joined {playerRef}");

            if (runner.IsSharedModeMasterClient)
            {
                runner.RemoveCallbacks(this);

                PlayerManager.AddPlayerForQueue(playerRef);

                if (PlayerManager.PlayerQueue.Count == 1)
                {
                    await Task.Delay(500);

                    FindObjectOfType<SceneLoader>().LoadScene("Main");
                }
            }
        }

        public async void CreateSession(string levelName, MapType mapType)
        {
            var startGameArgs = new StartGameArgs();

            _runner.AddCallbacks(this);

            startGameArgs.GameMode = GameMode.Shared;
            startGameArgs.SessionName = $"{levelName} : {mapType},{Guid.NewGuid()}";
            startGameArgs.SceneManager = FindObjectOfType<SceneLoader>();
            startGameArgs.Scene = SceneManager.GetActiveScene().buildIndex;

            startGameArgs.SessionProperties = new Dictionary<string, SessionProperty>()
            {
                ["map"] = $"{levelName}",
                ["mode"] = (int)mapType
            };

            StartGameResult startGameResult = await _runner.StartGame(startGameArgs);
        }

        public void JoinSession(string sessionName)
        {
            var startGameArgs = new StartGameArgs();

            startGameArgs.GameMode = GameMode.Shared;
            startGameArgs.SessionName = sessionName;
            startGameArgs.SceneManager = FindObjectOfType<SceneLoader>();

            _runner.StartGame(startGameArgs);
        }

        public void JoinPickedSession()
        {
            if(PickedSessionId == -1) return;
            
            SessionGameInfo sessionGameInfo = GetSessionInfoById(PickedSessionId);

            JoinSession(sessionGameInfo.SessionName);
        }

        public SessionGameInfo GetSessionInfoById(int sessionId)
        {
            return _sessionGameInfos[sessionId];
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

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}