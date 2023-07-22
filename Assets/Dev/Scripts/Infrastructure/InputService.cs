using System;
using System.Collections.Generic;
using Dev.Infrastructure;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Dev
{
    public class InputService : NetworkContext, INetworkRunnerCallbacks
    {
        private Joystick _aimJoystick;
        private Joystick _movementJoystick;

        public void Init(Joystick movementJoystick, Joystick aimJoystick)
        {
            _movementJoystick = movementJoystick;
            _aimJoystick = aimJoystick;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");
            
            //var moveDirection = new Vector2(x, y);
            var moveDirection = _movementJoystick.Direction;
            moveDirection.Normalize();
            
            Vector2 lookDirection = _aimJoystick.Direction;
            lookDirection.Normalize();
            
            PlayerInput playerInput = new PlayerInput();
            playerInput.MoveDirection = moveDirection;
            playerInput.LookDirection = lookDirection;

            input.Set(playerInput);
        }

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