using Dev.Infrastructure;
using UnityEngine;

namespace Dev
{
    public class Test : NetworkContext
    {
        [SerializeField] private NetworkCharacterControllerPrototype _controllerPrototype;

        [SerializeField] private Vector3 _moveDirection;
        
        [SerializeField] private float _speed = 50;
        
        [ContextMenu("MakeThisObjectMine")]
        private void MakeThisMine()
        {
            Runner.ProvideInput = true;
            Object.AssignInputAuthority(Runner.LocalPlayer);
        }

        public override void Spawned()
        {
            Runner.ProvideInput = true;
            Object.AssignInputAuthority(Runner.LocalPlayer);
        }

        public override void FixedUpdateNetwork()
        {
            var hasInput = GetInput<PlayerInput>(out var input);

            if (hasInput == false) return;

            Debug.Log($"has input");
            
            _controllerPrototype.Move(_moveDirection * _speed * Runner.DeltaTime);

            _controllerPrototype.gravity = _moveDirection.y;
        }
    }
}