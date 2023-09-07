using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Dev.Infrastructure
{
    [RequireComponent(typeof(NetworkRunner))]
    public class FusionLobbyConnector : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _networkRunner;

        private async void Awake()
        {
            _networkRunner = GetComponent<NetworkRunner>();
            _networkRunner.ProvideInput = true;

            var gameSessionBrowser = FindObjectOfType<GameSessionBrowser>();

            _networkRunner.AddCallbacks(gameSessionBrowser);

            var joinSessionLobby = _networkRunner.JoinSessionLobby(SessionLobby.ClientServer);

            await joinSessionLobby;

            StartGameResult result = joinSessionLobby.Result;

            if (result.Ok)
            {
                Debug.Log($"Joined lobby");
            }
            else
            {
                Debug.LogError($"Failed to Start: {result.ShutdownReason}");
            }
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

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}

