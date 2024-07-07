using UnityEngine;

namespace Dev.UI
{
    public class JoysticksContainer : MonoBehaviour
    {
        [SerializeField] private Joystick _movementJoystick;
        [SerializeField] private Joystick _aimJoystick;

        public Joystick MovementJoystick => _movementJoystick;
        public Joystick AimJoystick => _aimJoystick;
    }
}