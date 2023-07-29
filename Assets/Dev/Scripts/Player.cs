using Dev.Infrastructure;
using Dev.Weapons;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class Player : NetworkContext
    {
        [SerializeField] private float _speed;
        [Range(0f, 1f)] [SerializeField] private float _shootThreshold = 0.75f;
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] private HitboxRoot _hitboxRoot;
        [SerializeField] private WeaponController _weaponController;

        public PlayerView PlayerView => _playerView;

        public HitboxRoot HitboxRoot => _hitboxRoot;

        public Rigidbody2D Rigidbody => _networkRigidbody2D.Rigidbody;

        [Networked] private Vector2 LastMoveDirection { get; set; }
        [Networked] private Vector2 LastLookDirection { get; set; }

        public override void FixedUpdateNetwork()
        {
            var hasInput = GetInput<PlayerInput>(out var input);

            if (hasInput == false) return;

            Rigidbody.velocity = input.MoveDirection * _speed * Runner.DeltaTime;

            HandleAnimation(input);

            AimRotation(input);
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

            _playerView.OnMove(moveDirection.magnitude, isRight);
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