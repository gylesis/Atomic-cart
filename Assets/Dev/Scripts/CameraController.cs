using Dev.Infrastructure;
using Dev.PlayerLogic;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class CameraController : NetworkContext
    {
        [SerializeField] private float _followSpeed = 1.5f;
        [SerializeField] private Camera _camera;

        private Transform _target;
        private GameSettings _gameSettings;
        private bool _toFollow;
        private CameraService _cameraService;

        [Inject]
        private void Construct(CameraService cameraService, GameSettings gameSettings)
        {
            _cameraService = cameraService;
            _gameSettings = gameSettings;
        }
        
        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                _cameraService.SetMainCameraState(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SetupTarget(Transform target)
        {
            _target = target;
        }
        
        public void SetFollowState(bool toFollow)
        {
            _toFollow = toFollow;
        }

        public void FastSetOnTarget()
        {
            transform.position = _target.position;
        }
        
        public override void Render()
        {
            if (HasStateAuthority == false) return;

            _camera.orthographicSize = 
                _gameSettings.CameraZoomModifier;
            
            FollowTarget();
        }

        private void FollowTarget()
        {
            if(_toFollow == false) return;
            
            if (_target == null) return;
            
            transform.position = Vector3.Lerp(transform.position, _target.position, Runner.DeltaTime * _gameSettings.CameraFollowSpeed);
        }
    }
}