using Dev.Infrastructure;
using Dev.Utils;
using UnityEngine;

namespace Dev
{
    public class Test : NetworkContext
    {
        [SerializeField] private Transform _pos;
        [SerializeField] private float _radius = 2;
        [SerializeField] private bool _cast = false;
        
        [ContextMenu("Test")]
        private void Hit()
        {
            Extensions.OverlapCircleExcludeWalls(Runner, _pos.position, _radius, out var collider);
        }

        public override void Render()
        {
            if(!_cast) return;
            
            Hit();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_pos.position, _radius);
        }
    }
}