using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dev.UI;
using Fusion;
using Fusion.Sockets;
using UniRx;
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

        private ObjectPool<SessionUIView> _sessionUIViewPool;

        public int PickedSessionId { get; private set; }

        private List<SessionGameInfo> _sessionGameInfos = new List<SessionGameInfo>(8);

        public Subject<int> SessionCountChanged { get; } = new Subject<int>();

        private List<SessionUIView> _sessionUIViews = new List<SessionUIView>(8);
        private NetworkRunner _networkRunner;
        private AuthService _authService;

        private void Awake()
        {
            _networkRunner = FindObjectOfType<NetworkRunner>();
            _networkRunner.AddCallbacks(this);
            
            _sessionUIViewPool =
                new ObjectPool<SessionUIView>(CreateFunc, ActionOnGet, ActionOnRelease);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _networkRunner.RemoveCallbacks(this);
            base.Despawned(runner, hasState);
        }

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
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

        private void OnSessionClicked(SessionUIView obj)
        {
            PickedSessionId = obj.Id;
        }

        public async UniTask<StartGameResult> CreateSession(string levelName, MapType mapType)
        {
            var startGameArgs = new StartGameArgs();

            _networkRunner.AddCallbacks(this);

            startGameArgs.GameMode = GameMode.Shared;
            startGameArgs.SessionName = $"{_authService.MyProfile.Nickname}";
            startGameArgs.SceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            Scene activeScene = SceneManager.GetActiveScene();
            startGameArgs.Scene = SceneRef.FromIndex(activeScene.buildIndex);

            startGameArgs.SessionProperties = new Dictionary<string, SessionProperty>()
            {
                ["map"] = $"{levelName}",
                ["mode"] = (int)mapType,
                ["status"] = (int)SessionStatus.Lobby
            };

            return await _networkRunner.StartGame(startGameArgs);
        }

        public void JoinSession(string sessionName)
        {
            var startGameArgs = new StartGameArgs();

            startGameArgs.GameMode = GameMode.Shared;
            startGameArgs.SessionName = sessionName;
            startGameArgs.SceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            _networkRunner.StartGame(startGameArgs);
        }

        public void JoinPickedSession()
        {
            if (PickedSessionId == -1) return;

            SessionGameInfo sessionGameInfo = GetSessionInfoById(PickedSessionId);

            JoinSession(sessionGameInfo.SessionName);
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

                sessionGameInfo.Id = index;
                sessionGameInfo.SessionName = sessionInfo.Name;
                sessionGameInfo.CurrentPlayers = sessionInfo.PlayerCount;
                sessionGameInfo.MaxPlayers = sessionInfo.MaxPlayers;
                sessionGameInfo.MapType = (MapType)sessionInfo.Properties["mode"].PropertyValue;
                sessionGameInfo.SessionStatus = (SessionStatus)sessionInfo.Properties["status"].PropertyValue;
                sessionGameInfo.MapName = sessionInfo.Properties["map"].PropertyValue.ToString();

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

        public SessionGameInfo GetSessionInfoById(int sessionId)
        {
            return _sessionGameInfos[sessionId];
        }

        public async void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
                                     byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
                                           ArraySegment<byte> data) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}