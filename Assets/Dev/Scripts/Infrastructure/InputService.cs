using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class InputService : NetworkContext, INetworkRunnerCallbacks
    {
        private Joystick _aimJoystick;
        private Joystick _movementJoystick;

        public override void Spawned()
        {
            Runner.AddCallbacks(this);

            var playersSpawner = FindObjectOfType<PlayersSpawner>();

            _aimJoystick = playersSpawner.AimJoystick;
            _movementJoystick = playersSpawner.MovementJoystick;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            Vector2 joystickDirection = _movementJoystick.Direction;

            var moveDirection = joystickDirection;
            
            if (joystickDirection == Vector2.zero)
            {
                var x = Input.GetAxis("Horizontal");
                var y = Input.GetAxis("Vertical");

                var keyBoardInput = new Vector2(x, y);
                moveDirection = keyBoardInput;
                
                //moveDirection.Normalize();
            }

            Vector2 aimJoystickDirection = _aimJoystick.Direction;

            PlayerInput playerInput = new PlayerInput();
            
            playerInput.MoveDirection = moveDirection;
            playerInput.LookDirection = aimJoystickDirection;

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