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

        public void SetupTarget(Transform target)
        {
            _target = target;
        }

        private void Start()
        {
            _gameSettings = DependenciesContainer.Instance.GetDependency<GameSettings>();
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                DependenciesContainer.Instance.GetDependency<CameraService>().SetMainCameraState(false);

                SetupTarget(Player.LocalPlayer.transform);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public override void Render()
        {
            if (HasInputAuthority == false) return;

            _camera.orthographicSize = _gameSettings.CameraZoomModifier;
            
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