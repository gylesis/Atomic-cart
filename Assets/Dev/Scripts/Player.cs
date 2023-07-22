using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class Player : NetworkContext
    {
        [SerializeField] private float _speed;
        
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;

        private Rigidbody2D Rigidbody => _networkRigidbody2D.Rigidbody;

        public override void FixedUpdateNetwork()
        {
            var hasInput = GetInput<PlayerInput>(out var input);

            if (hasInput == false) return;
            
            Rigidbody.velocity = input.MoveDirection * _speed * Runner.DeltaTime;

            Vector2 lookDirection = input.LookDirection;
            
            if (input.LookDirection == Vector2.zero)
            {
                lookDirection = transform.up;
            }

            transform.up = lookDirection;
        }
    }
}