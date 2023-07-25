using Dev.Infrastructure;
using Dev.Weapons;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class Player : NetworkContext
    {
        [SerializeField] private float _speed;
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] private WeaponController _weaponController;

        [SerializeField] private HitboxRoot _hitboxRoot;

        public HitboxRoot HitboxRoot => _hitboxRoot;

        private Vector2 ShootDirection => transform.up;
        
        public Rigidbody2D Rigidbody => _networkRigidbody2D.Rigidbody;

        public override void FixedUpdateNetwork()
        {
            var hasInput = GetInput<PlayerInput>(out var input);

            if (hasInput == false) return;
            
            Rigidbody.velocity = input.MoveDirection * _speed * Runner.DeltaTime;

            AimRotation(input);
        }

        private void AimRotation(PlayerInput input)
        {
            Vector2 lookDirection = input.LookDirection;

            if (input.LookDirection == Vector2.zero)
            {
                lookDirection = transform.up;
            }
            else
            {
                var magnitude = lookDirection.sqrMagnitude;

                if (magnitude >= 0.75f)
                {
                    Shoot();
                }
                
            }
            transform.up = lookDirection;
        }

        private void Shoot()
        {
            _weaponController.TryToFire(ShootDirection);
        }
        
    }
    
}