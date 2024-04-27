using System.Collections.Generic;
using Dev.UI;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class InputService : ITickable, IInitializable
    {
        private Joystick _aimJoystick;
        private Joystick _movementJoystick;

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

        public void Initialize()
        {
            _aimJoystick = _joysticksContainer.AimJoystick;
            _movementJoystick = _joysticksContainer.MovementJoystick;
        }

        public void Tick()
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
            //playerInput.WeaponNum = -1;

            playerInput.CastAbility = Input.GetKeyDown(KeyCode.F);
            

            for (int i = 0; i < _keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(_keyCodes[i]))
                {
                    int numberPressed = i + 1;
                    playerInput.WeaponNum = numberPressed;
                }
            }

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