using Dev.Infrastructure;
using UnityEngine;

namespace Dev
{
    public class CameraController : NetworkContext
    {
        [SerializeField] private float _followSpeed = 1.5f;
        [SerializeField] private Camera _camera;
        
        private Transform _target;
        
        public void SetupTarget(Transform target)
        {
            _target = target;
        }

        public override void Render()
        {
            if (HasInputAuthority == false) return;
            
            if (_target == null) return;
            
            transform.position = Vector3.Lerp(transform.position , _target.position, Time.deltaTime * _followSpeed);
        }
    }
}