using System;
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

        public override void Spawned()
        {
            if (HasInputAuthority)
            {
                FindObjectOfType<CameraService>().RPC_SetMainCameraState(false);
                
                SetupTarget(Runner.GetPlayerObject(Runner.LocalPlayer).transform);
            }
            else
            {
                gameObject.SetActive(false);
            }

        }

        public override void Render()
        {
            if (HasInputAuthority == false) return;
            
            FollowTarget();
        }

        private void Update()
        {
            FollowTarget();
        }

        private void FollowTarget()
        {
            if (_target == null) return;

            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * _followSpeed);
        }
    }
}