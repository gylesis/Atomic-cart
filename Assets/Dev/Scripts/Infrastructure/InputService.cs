using System;
using System.Collections.Generic;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class InputService : PlayerService, INetworkRunnerCallbacks
    {
        private Joystick _aimJoystick;
        private Joystick _movementJoystick;
        private PopUpService _popUpService;

        private KeyCode[] _keyCodes =
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
        };

        private void Awake()
        {
            _popUpService = FindObjectOfType<PopUpService>();
        }

        public override void Spawned()
        {
            Runner.AddCallbacks(this);

            JoysticksContainer joysticksContainer = DependenciesContainer.Instance.GetDependency<JoysticksContainer>();

            _aimJoystick = joysticksContainer.AimJoystick;
            _movementJoystick = joysticksContainer.MovementJoystick;
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
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
            playerInput.WeaponNum = 22;

            for (int i = 0; i < _keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(_keyCodes[i]))
                {
                    int numberPressed = i + 1;
                    playerInput.WeaponNum = numberPressed;
                }
            }

            input.Set(playerInput);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}