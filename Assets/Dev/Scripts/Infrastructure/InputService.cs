using System.Collections.Generic;
using Dev.UI;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class InputService : ITickable
    {
        private Joystick AimJoystick => _joysticksContainer.AimJoystick;
        private Joystick MovementJoystick => _joysticksContainer.MovementJoystick;

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


        public Queue<PlayerInput> BufferedInputs = new Queue<PlayerInput>(16);
        private JoysticksContainer _joysticksContainer;

        public InputService(JoysticksContainer joysticksContainer)
        {
            _joysticksContainer = joysticksContainer;
        }

        public void Tick()
        {
            Vector2 movementJoystickDirection = MovementJoystick.Direction;

            Vector2 moveDirection = movementJoystickDirection;

            if (movementJoystickDirection == Vector2.zero)
            {
                var x = Input.GetAxis("Horizontal");
                var y = Input.GetAxis("Vertical");

                var keyBoardInput = new Vector2(x, y);
                moveDirection = keyBoardInput;

                //moveDirection.Normalize();
            }

            Vector2 aimJoystickDirection = AimJoystick.Direction;

            PlayerInput playerInput = new PlayerInput();

            playerInput.WithMoveDirection(moveDirection);
            playerInput.WithAimDirection(aimJoystickDirection);
            playerInput.WithCast(Input.GetKeyDown(KeyCode.F));
            

            if (moveDirection == Vector2.zero && aimJoystickDirection == Vector2.zero)
            {
                //return;
            }

            BufferedInputs.Enqueue(playerInput);
        }

        public void SimulateInput(PlayerInput playerInput)
        {
            BufferedInputs.Enqueue(playerInput);
        }
        
    }
}