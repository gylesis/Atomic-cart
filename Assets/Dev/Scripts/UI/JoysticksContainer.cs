using UnityEngine;

namespace Dev.UI
{
    public class JoysticksContainer : MonoBehaviour
    {
        [SerializeField] private Joystick _movementJoystick;
        [SerializeField] private AimJoystick _aimJoystick;
    
        public Joystick MovementJoystick => _movementJoystick;
        public AimJoystick AimJoystick => _aimJoystick;
    }
}   