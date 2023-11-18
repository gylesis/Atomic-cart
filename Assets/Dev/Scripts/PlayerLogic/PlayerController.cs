using System.Numerics;
using System.Threading.Tasks;
using Dev.Infrastructure;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Weapons;
using Fusion;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Dev.PlayerLogic
{
    public class PlayerController : NetworkContext
    {
        [SerializeField] private PlayerCharacter _playerCharacter;
        private WeaponController _weaponController => _playerCharacter.WeaponController;
        private PlayerView PlayerView => _playerCharacter.PlayerView;

        [Networked] private Vector2 LastMoveDirection { get; set; }
        [Networked] public Vector2 LastLookDirection { get; private set; }

        [Networked] public NetworkBool IsPlayerAiming { get; private set; }

        [Networked(OnChanged = nameof(OnAllowToMoveChanged))]
        public NetworkBool AllowToMove { get; set; } = true;

        [Networked(OnChanged = nameof(OnAllowToShootChanged))]
        public NetworkBool AllowToShoot { get; set; } = true;

        [SerializeField] private float _shiftSpeed = 2;
        [SerializeField] private float _dashTime = 0.5f;

        private float _speed;
        private float _shootThreshold;
        private float _speedLowerSpeed;

        private PopUpService _popUpService;
        private JoysticksContainer _joysticksContainer;

        [Networked] private Vector2 LookDirection { get; set; }
        [Networked] private Vector2 MoveDirection { get; set; }

        private TickTimer _dashTimer;

        private void Awake()
        {
            _popUpService = DependenciesContainer.Instance.GetDependency<PopUpService>();
            _joysticksContainer = DependenciesContainer.Instance.GetDependency<JoysticksContainer>();
        }

        [Rpc]
        public void RPC_Init(float moveSpeed, float shootThreshold, float speedLowerVelocity)
        {
            _speed = moveSpeed;
            _shootThreshold = shootThreshold;
            _speedLowerSpeed = speedLowerVelocity;
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput<PlayerInput>(out var input))
            {
                MoveDirection = input.MoveDirection;
                LookDirection = input.LookDirection;
            }

            Vector2 moveDirection = MoveDirection;

            if (AllowToMove)
            {
                if (_dashTimer.ExpiredOrNotRunning(Runner))
                {
                    if (moveDirection != Vector2.zero)
                    {
                        Vector2 velocity = moveDirection * (_speed * Runner.DeltaTime);

                        _playerCharacter.Rigidbody.velocity = velocity;
                    }

                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        Dash();
                    }
                }
            }

            if (moveDirection == Vector2.zero)
            {
                Vector2 velocity = _playerCharacter.Rigidbody.velocity;

                if (velocity.sqrMagnitude != 0)
                {
                    float lowerModifier = (_speedLowerSpeed * Runner.DeltaTime);

                    float xSign = Mathf.Sign(velocity.x) == 1 ? 1 : -1;
                    float ySign = Mathf.Sign(velocity.y) == 1 ? 1 : -1;

                    //Debug.Log($"xSign {xSign}, ySign {ySign}, x vel: {velocity.x}, y vel {velocity.y}");

                    velocity.x *= lowerModifier * xSign;
                    velocity.y *= lowerModifier * ySign;

                    velocity.x = Mathf.Clamp(velocity.x, 0, float.MaxValue);
                    velocity.y = Mathf.Clamp(velocity.y, 0, float.MaxValue);

                    _playerCharacter.Rigidbody.velocity = velocity;
                }
            }

            HandleAnimation();

            if (AllowToShoot)
            {
                AimRotation();
            }
        }

        private async void Dash()
        {
            float dashTime = 0.5f;

            float dashDistance = 2;
            
            _dashTimer = TickTimer.CreateFromSeconds(Runner, dashTime);

            Vector3 targetPos = _playerCharacter.transform.position + (Vector3) LastMoveDirection.normalized * dashDistance;

            
            
            
            
            float stepPerTick = 0.05f;
            int stepsCount = (int)(dashTime / stepPerTick);

            for (int i = 0; i < stepsCount; i++)
            {
                float force = 1 - (i / stepsCount);

                force *= _shiftSpeed * Runner.DeltaTime;

                _playerCharacter.Rigidbody.velocity += LastLookDirection * force;

                await Task.Yield();
            }
        }

        public static void OnAllowToMoveChanged(Changed<PlayerController> changed)
        {
            changed.Behaviour._joysticksContainer.MovementJoystick.gameObject.SetActive(changed.Behaviour.AllowToMove);
        }

        public static void OnAllowToShootChanged(Changed<PlayerController> changed)
        {
            changed.Behaviour._joysticksContainer.AimJoystick.gameObject.SetActive(changed.Behaviour.AllowToShoot);
        }

        public override void Render()
        {
            return;
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                var tryGetPopUp = _popUpService.TryGetPopUp<PlayersScoreMenu>(out var scoreMenu);
                scoreMenu.Show();
            }

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                var tryGetPopUp = _popUpService.TryGetPopUp<PlayersScoreMenu>(out var scoreMenu);
                scoreMenu.Hide();
            }
        }

        private void HandleAnimation()
        {
            Vector2 moveDirection = MoveDirection;

            float sign = 1;

            if (moveDirection == Vector2.zero)
            {
                sign = Mathf.Sign(Vector2.Dot(Vector2.left, LastMoveDirection));
            }
            else
            {
                LastMoveDirection = moveDirection;
                sign = Mathf.Sign(Vector2.Dot(Vector2.left, moveDirection));
            }

            bool isRight = sign == 1;

            PlayerView.OnMove(moveDirection.magnitude, isRight);
        }

        private void AimRotation()
        {
            if (_weaponController.HasAnyWeapon == false) return;

            Vector2 lookDirection = LookDirection;

            if (LookDirection == Vector2.zero)
            {
                IsPlayerAiming = false;
                lookDirection = LastLookDirection;
            }
            else
            {
                IsPlayerAiming = true;

                LastLookDirection = lookDirection;

                var magnitude = lookDirection.sqrMagnitude;

                if (magnitude >= _shootThreshold)
                {
                    Shoot();
                }
            }


            _weaponController.AimWeaponTowards(lookDirection);
        }

        private void Shoot()
        {
            _weaponController.TryToFire();
        }
    }
}