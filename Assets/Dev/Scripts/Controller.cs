using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class Controller : NetworkContext
    {
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;

        [SerializeField] private float _moveSpeed = 5    ;
        
        public override void FixedUpdateNetwork()
        {
            
            
        }
    }
}