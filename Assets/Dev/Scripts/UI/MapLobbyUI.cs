using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev.Infrastructure;
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

        [Networked, Capacity(8)] private NetworkLinkedList<ReadyUI> ReadyUis => default;

        [SerializeField]
        [Networked, Capacity(8)] public NetworkDictionary<PlayerRef, bool> ReadyStatesDictionary => default;

        [SerializeField] private LobbyPlayer _lobbyPlayer;
        private PopUpService _popUpService;

        public Subject<Unit> ReadyStatusUpdated { get; } = new Subject<Unit>();

        public override void Spawned()
        {
            base.Spawned();

            if (Runner.IsSharedModeMasterClient)
            {
                foreach (ReadyUI readyUi in _readyUis)
                {
                    ReadyUis.Add(readyUi);
                }
            }
            
        }

        [Inject]
        private void Init(NetworkRunner runner, PopUpService popUpService)
        {
            _popUpService = popUpService;
            runner.AddCallbacks(this);
        }

        [Rpc]
        public void RPC_SetReady(PlayerRef playerRef)
        {
            NetworkBool wasReady = IsPlayerReady(playerRef);
            ReadyStatesDictionary.Set(playerRef, !wasReady);
            ReadyStatusUpdated.OnNext(Unit.Default);

            ReadyUis.First(x => x.PlayerRef == playerRef).RPC_SetReadyView(!wasReady);
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
        
        public async void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            await Task.Delay(500);

            ReadyUI readyUI = ReadyUis.First(x => x.PlayerRef == PlayerRef.None);
            
            readyUI.RPC_AssignPlayer(playerRef);
            

            Debug.Log($"Player {playerRef} joined to lobby", readyUI);
            
            if (runner.IsSharedModeMasterClient)
            {
               // LobbyPlayer lobbyPlayer = runner.Spawn(_lobbyPlayer, null, null, playerRef);
               // runner.SetPlayerObject(playerRef, lobbyPlayer.Object);
                
               PlayerManager.AddPlayerForQueue(playerRef);
            }
            
            ReadyStatesDictionary.Add(playerRef, false);

            return;
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


        public void OnConnectedToServer(NetworkRunner runner)
        {
            _popUpService.HidePopUp<LobbyPlayMenu>();
            _popUpService.ShowPopUp<MapLobbyMenu>();
            Debug.Log($"{runner.LocalPlayer} Connected to server");
        }   
        
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

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

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log($"Scene load start");
          //  runner.RemoveCallbacks(this);
        }
    }
}