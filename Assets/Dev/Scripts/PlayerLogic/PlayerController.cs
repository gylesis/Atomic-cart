using Dev.Infrastructure;
using Dev.UI;
using Dev.Weapons;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class PlayerController : NetworkContext
    {
        [SerializeField] private Player _player;
       
        private WeaponController _weaponController => _player.WeaponController;
        private PlayerView PlayerView => _player.PlayerView;

        [Networked] private Vector2 LastMoveDirection { get; set; }
        [Networked] private Vector2 LastLookDirection { get; set; }

        public bool AllowToMove { get; set; } = true;
        public bool AllowToShoot { get; set; } = true;

        private float Speed => _characterStats.MoveSpeed;
        private float ShootThreshold => _characterStats.ShootThreshold;
        private float SpeedLowerSpeed => _characterStats.SpeedLowerSpeed;
        
        private PopUpService _popUpService;
        private CharacterStats _characterStats;

        private void Awake()
        {
            _popUpService = DependenciesContainer.Instance.GetDependency<PopUpService>();
        }

        public void Init(CharacterStats characterStats)
        {
            _characterStats = characterStats;
        }
        
        public override void FixedUpdateNetwork()
        {
            var hasInput = GetInput<PlayerInput>(out var input);

            if (hasInput == false) return;

            if (AllowToMove)
            {
                if (input.MoveDirection != Vector2.zero)
                {
                    Vector2 velocity = input.MoveDirection * (Speed * Runner.DeltaTime);
                    _player.Rigidbody.velocity = velocity;
                }
            }

            if (input.MoveDirection == Vector2.zero)
            {
                Vector2 velocity = _player.Rigidbody.velocity;

                if (velocity.sqrMagnitude != 0)
                {
                    float lowerModifier = (SpeedLowerSpeed * Runner.DeltaTime);
                    
                    float xSign = Mathf.Sign(velocity.x) == 1 ? 1 : -1;
                    float ySign = Mathf.Sign(velocity.y) == 1 ? 1 : -1;

                    //Debug.Log($"xSign {xSign}, ySign {ySign}, x vel: {velocity.x}, y vel {velocity.y}");
                    
                    velocity.x *= lowerModifier * xSign;
                    velocity.y *= lowerModifier * ySign;
                    
                    velocity.x = Mathf.Clamp(velocity.x, 0, float.MaxValue);
                    velocity.y = Mathf.Clamp(velocity.y, 0, float.MaxValue);
                    
                    _player.Rigidbody.velocity = velocity;
                }
            }


            HandleAnimation(input);

            if (AllowToShoot)
            {
                AimRotation(input);
            }
        }

        public override void Render()
        {
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

        private void HandleAnimation(PlayerInput input)
        {
            Vector2 moveDirection = input.MoveDirection;

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

        private void AimRotation(PlayerInput input)
        {
            if (_weaponController.HasAnyWeapon == false) return;

            Vector2 lookDirection = input.LookDirection;

            if (input.LookDirection == Vector2.zero)
            {
                lookDirection = LastLookDirection;
            }
            else
            {
                LastLookDirection = lookDirection;

                var magnitude = lookDirection.sqrMagnitude;

                if (magnitude >= ShootThreshold)
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