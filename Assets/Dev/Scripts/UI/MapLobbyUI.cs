using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev.Infrastructure;
using Dev.UI.PopUpsAndMenus;
using Fusion;
using Fusion.Sockets;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI
{
    public class MapLobbyUI : NetworkContext, INetworkRunnerCallbacks
    {
        [SerializeField] private ReadyUI[] _readyUis;

        [Networked, Capacity(8)] public NetworkDictionary<PlayerRef, bool> ReadyStatesDictionary => default;

        public Subject<Unit> ReadyStatusUpdated { get; } = new Subject<Unit>();

        private void Awake()
        {
            FindObjectOfType<NetworkRunner>().AddCallbacks(this);
        }

        protected override void CorrectState()
        {
            base.CorrectState();

            foreach (var valuePair in ReadyStatesDictionary)
            {
                bool isReady = valuePair.Value;
                PlayerRef playerRef = valuePair.Key;

                ReadyUI readyUI = _readyUis.First(x => x.PlayerRef == playerRef);

                readyUI.RPC_SetReadyView(isReady);
            }
        }
        

        public void SetReady(PlayerRef playerRef)
        {
            NetworkBool wasReady = IsPlayerReady(playerRef);

            RPC_ReadyStatusUpdate(!wasReady, playerRef);

            _readyUis.First(x => x.PlayerRef == playerRef).RPC_SetReadyView(!wasReady);
        }

        [Rpc]
        private void RPC_ReadyStatusUpdate(bool isReady, PlayerRef playerRef)
        {
            ReadyStatesDictionary.Set(playerRef, isReady);

            ReadyStatusUpdated.OnNext(Unit.Default);
        }

        public bool IsPlayerReady(PlayerRef playerRef)
        {
            return ReadyStatesDictionary[playerRef];
        }

        public bool IsAllPlayersReady()
        {
            int readyPlayerCount = ReadyStatesDictionary.Count(x => x.Value);
            int requiredPlayersCountToStart = ReadyStatesDictionary.Count;

            Debug.Log($"Ready players {readyPlayerCount}");

            return readyPlayerCount == requiredPlayersCountToStart;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            PlayerManager.LoadingPlayers.Add(playerRef);
            PlayerManager.PlayersOnServer.Add(playerRef);
            
            if (HasStateAuthority)
            {
                ReadyUI readyUI = _readyUis.First(x => x.PlayerRef == PlayerRef.None);

                Debug.Log($"Player {playerRef} joined to lobby", readyUI);
               
                readyUI.RPC_AssignPlayer(playerRef);

                ReadyStatesDictionary.Add(playerRef, false);
            }
            else
            {
                PlayersManager.Instance.RPC_Register(playerRef, AuthService.Nickname);
            }

            ReadyStatusUpdated.OnNext(Unit.Default);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            PlayerManager.LoadingPlayers.Remove(playerRef);
            PlayerManager.PlayersOnServer.Remove(playerRef);

            ReadyUI readyUI = _readyUis.First(x => x.PlayerRef == PlayerRef.None);

            Debug.Log($"Player {playerRef} joined to lobby", readyUI);

            readyUI.RPC_RemovePlayerAssigment(playerRef);

            ReadyStatesDictionary.Remove(playerRef);

            ReadyStatusUpdated.OnNext(Unit.Default);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            PopUpService.Instance.HidePopUp<LobbyPlayMenu>();
            PopUpService.Instance.ShowPopUp<SessionMenu>();

            Debug.Log($"{runner.LocalPlayer} Connected to server");
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

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

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log($"Scene load start");
            //  runner.RemoveCallbacks(this);
        }
    }
}