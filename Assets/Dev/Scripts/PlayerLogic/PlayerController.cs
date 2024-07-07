using System;
using System.Threading.Tasks;
using Dev.Infrastructure;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Weapons;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Dev.PlayerLogic
{
    public class PlayerController : NetworkContext
    {
        [SerializeField] private float _shiftSpeed = 2;
        [SerializeField] private float _dashTime = 0.5f;
        
        private float _speed;
        private float _shootThreshold;
        private float _speedLowerSpeed;
        private Action _onActionButtonPressed;
        private TickTimer _dashTimer;

        private PopUpService _popUpService;
        private JoysticksContainer _joysticksContainer;
        private PlayerBase _playerBase;
        private InputService _inputService;
        
        private PlayerCharacter PlayerCharacter => _playerBase.Character;
        private WeaponController _weaponController => PlayerCharacter.WeaponController;
        private PlayerView PlayerView => PlayerCharacter.PlayerView;

        public bool AllowToMove { get; private set; } = true;
        public bool AllowToShoot { get; private set; } = true;
        
        [Networked] private Vector2 LastMoveDirection { get; set; }
        [Networked] public Vector2 LastLookDirection { get; private set; }
        [Networked] public NetworkBool IsPlayerAiming { get; private set; }
        [Networked] private Vector2 LookDirection { get; set; }
        [Networked] private Vector2 MoveDirection { get; set; }

        [Inject]
        private void Construct(PopUpService popUpService, JoysticksContainer joysticksContainer, InputService inputService, PlayerBase playerBase)
        {
            _playerBase = playerBase;
            _inputService = inputService;
            _popUpService = popUpService;
            _joysticksContainer = joysticksContainer;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            bool tryGetPopUp = _popUpService.TryGetPopUp<HUDMenu>(out var hudMenu);

            if (tryGetPopUp)
            {
                hudMenu.InteractiveButtonClicked.TakeUntilDestroy(this)
                    .Subscribe((unit => OnInteractionButtonClicked()));
            }

            SetInteractionAction(null);
        }

        public void Init(float moveSpeed, float shootThreshold, float speedLowerVelocity)
        {
            _speed = moveSpeed;
            _shootThreshold = shootThreshold;
            _speedLowerSpeed = speedLowerVelocity;
        }

        private void OnInteractionButtonClicked()
        {
            _onActionButtonPressed?.Invoke();
        }

        public void SetInteractionAction(Action onActionButtonPressed)
        {
            _onActionButtonPressed = onActionButtonPressed;

            bool tryGetPopUp = _popUpService.TryGetPopUp<HUDMenu>(out var hudMenu);

            if (tryGetPopUp)
            {
                hudMenu.InteractiveButtonClicked.TakeUntilDestroy(this)
                    .Subscribe((unit => OnInteractionButtonClicked()));

                hudMenu.SetInteractionButtonState(onActionButtonPressed == null);
            }
        }

        private void Shoot()
        {
            _weaponController.TryToFire();
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            foreach (PlayerInput input in _inputService.BufferedInputs)
            {
                MoveDirection = input.MoveDirection;
                LookDirection = input.AimDirection;

                Vector2 moveDirection = MoveDirection;

                if (AllowToMove)
                {
                    if (_dashTimer.ExpiredOrNotRunning(Runner))
                    {
                        if (moveDirection != Vector2.zero)
                        {
                            Vector2 velocity = moveDirection * (_speed * Runner.DeltaTime);

                            PlayerCharacter.Rigidbody.velocity = velocity;
                        }

                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            //Dash();
                        }
                    }
                }

                if (moveDirection == Vector2.zero)
                {
                    Vector2 velocity = PlayerCharacter.Rigidbody.velocity;

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

                        PlayerCharacter.Rigidbody.velocity = velocity;
                    }
                }

                if (AllowToShoot)
                {
                    if (input.ToCastAbility)
                    {
                        AbilityCastController castController = _playerBase.AbilityCastController;

                        castController.CastAbility(_playerBase.Character.transform.position + (Vector3)LastLookDirection.normalized * 6);
                    }

                    if (input.ToResetAbility)
                    {
                        AbilityCastController castController = _playerBase.AbilityCastController;

                        castController.ResetAbility();
                    }

                    AimRotation();
                }
            }

            // Debug.Log($"Destroyed {_inputService.BufferedInputs.Count} inputs");
            _inputService.BufferedInputs.Clear();
        }

        private async void Dash()
        {
            float dashTime = 0.5f;

            float dashDistance = 2;

            _dashTimer = TickTimer.CreateFromSeconds(Runner, dashTime);

            Vector3 targetPos = PlayerCharacter.transform.position +
                                (Vector3)LastMoveDirection.normalized * dashDistance;

            float stepPerTick = 0.05f;
            int stepsCount = (int)(dashTime / stepPerTick);

            for (int i = 0; i < stepsCount; i++)
            {
                float force = 1 - (i / stepsCount);

                force *= _shiftSpeed * Runner.DeltaTime;

                PlayerCharacter.Rigidbody.velocity += LastLookDirection * force;

                await Task.Yield();
            }
        }

        public void SetAllowToMove(bool allowToMove)
        {
            AllowToMove = allowToMove;
            _joysticksContainer.MovementJoystick.gameObject.SetActive(AllowToMove);
        }

        public void SetAllowToShoot(bool allowToShoot)
        {
            AllowToShoot = allowToShoot;
            _joysticksContainer.AimJoystick.gameObject.SetActive(AllowToShoot);
        }

        public override void Render()
        {
            HandleAnimation();
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
    }
}