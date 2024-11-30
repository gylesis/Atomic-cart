using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Fusion;
using Fusion.Sockets;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI
{
    public class SessionController : NetworkContext, INetworkRunnerCallbacks
    {
        [Networked, Capacity(8)] public NetworkDictionary<PlayerRef, bool> SessionPlayerReadyStatuses => default;
        public Subject<Unit> ReadyStatusUpdated { get; } = new Subject<Unit>();

        public Subject<PlayerRef> PlayerJoinedSession { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> PlayerLeftSession { get; } = new Subject<PlayerRef>();

        private AuthService _authService;

        protected override async void Awake()
        {
            base.Awake();

            LobbyConnector lobbyConnector = await LobbyConnector.WaitForInitialization();
            await UniTask.WaitUntil(() => lobbyConnector.NetworkRunner != null);
            lobbyConnector.NetworkRunner.AddCallbacks(this);
        }

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }

        public override async void Spawned()
        {
            var unityDataLinker = await UnityDataLinker.WaitForNetInitialization();
            unityDataLinker.RPC_Add(Runner.LocalPlayer, _authService.PlayerId);
            base.Spawned();
        }

        protected override void CorrectState()
        {
            base.CorrectState();
            ReadyStatusUpdated.OnNext(Unit.Default);
        }

        public void SetReady(PlayerRef playerRef)
        {
            NetworkBool wasReady = IsPlayerReady(playerRef);
            RPC_ReadyStatusUpdate(!wasReady, playerRef);
        }

        public bool IsPlayerReady(PlayerRef playerRef)
        {
            return SessionPlayerReadyStatuses[playerRef];
        }

        [Rpc]
        private void RPC_ReadyStatusUpdate(bool isReady, PlayerRef playerRef)
        {
            SessionPlayerReadyStatuses.Set(playerRef, isReady);
            ReadyStatusUpdated.OnNext(Unit.Default);
        }

        public bool IsAllPlayersReady()
        {
            int readyPlayerCount = SessionPlayerReadyStatuses.Count(x => x.Value);
            int requiredPlayersCountToStart = SessionPlayerReadyStatuses.Count;

            AtomicLogger.Log($"Ready players count {readyPlayerCount}");

            return readyPlayerCount == requiredPlayersCountToStart;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            PlayerManager.LoadingPlayers.Add(playerRef);
            PlayerManager.PlayersOnServer.Add(playerRef);

            AtomicLogger.Log($"Player {playerRef} joined to lobby");

            if (Runner.IsSharedModeMasterClient)
            {
                SessionPlayerReadyStatuses.Add(playerRef, false);
                PlayerJoinedSession.OnNext(playerRef);
            }
            else
            {
                //PlayersManager.Instance.RPC_Register(playerRef, AuthService.Nickname);
            }

            ReadyStatusUpdated.OnNext(Unit.Default);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            PlayerManager.LoadingPlayers.Remove(playerRef);
            PlayerManager.PlayersOnServer.Remove(playerRef);

            PlayerLeftSession.OnNext(playerRef);

            SessionPlayerReadyStatuses.Remove(playerRef);
            ReadyStatusUpdated.OnNext(Unit.Default);
        }


        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            PopUpService.Instance.HidePopUp<LobbyPlayMenu>();
            PopUpService.Instance.ShowPopUp<SessionMenu>();

            Debug.Log($"{runner.LocalPlayer} Connected to server");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
                                     byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

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