using System;
using System.Threading.Tasks;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class CameraController : PlayerService
    {
        [SerializeField] private float _followSpeed = 1.5f;
        [SerializeField] private Camera _camera;

        private Transform _target;
        private GameSettings _gameSettings;
        private bool _toFollow;

        public void SetupTarget(Transform target)
        {
            _target = target;
        }

        private void Start()
        {
            _gameSettings = GameSettingProvider.GameSettings;
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                DependenciesContainer.Instance.GetDependency<CameraService>().SetMainCameraState(false);

                SetupTarget(PlayerCharacter.LocalPlayerCharacter.transform);
            }
            else
            {
                gameObject.SetActive(false);
            }
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
            if (HasInputAuthority == false) return;

            _camera.orthographicSize = _gameSettings.CameraZoomModifier;
            
            FollowTarget();
        }

        private void FollowTarget()
        {
            if(_toFollow == false) return;
            
            if (_target == null) return;
            
            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * _followSpeed);
        }
    }
}