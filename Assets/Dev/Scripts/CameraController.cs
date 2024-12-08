using Dev.Infrastructure;
using Dev.Utils;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class CameraController : MonoContext
    {
        [SerializeField] private Transform _cameraLocalParent;
        [SerializeField] private float _followSpeed = 1.5f;
        [SerializeField] private Camera _camera;

        public Camera Camera => _camera;

        private bool _toFollow;
        private Transform _target;
        private Tweener _shakeTween;
        private Vector3 _originLocalPos;
        
        private GameSettings _gameSettings;

        protected override void Awake()
        {
            base.Awake();
            _originLocalPos = _cameraLocalParent.localPosition;
            transform.SetPositionZ(-10);
        }
        
        [Inject]
        private void Construct(GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
        }

        #region Shake
    
        public void Shake(string keyShakePattern)
        {
            if (GetShakeData(keyShakePattern, out var shakeData))
                Shake(shakeData);
            else
                AtomicLogger.Log($"Not found camera shake data for key {keyShakePattern}");
        }

        private bool GetShakeData(string key, out ShakeData shakeData)
        {
            shakeData = _gameSettings.CameraShakeConfig.GetData(key);
            return shakeData != null;
        }
        
        private void Shake(ShakeData shakeData)
        {
            _shakeTween?.Kill();
            
            float duration = shakeData.Duration;
            float power = shakeData.Power;
            Vector3 strenght = new Vector3(1, 1, 0);
            int vibrato = shakeData.Vibrato;
            float randomness = 180;

            _shakeTween = _cameraLocalParent.DOShakePosition(duration, strenght * power, vibrato, randomness).OnComplete(OnShakeComplete);
        }

        private void OnShakeComplete()
        {
            _cameraLocalParent.localPosition = _originLocalPos;
        }
        
        #endregion
        
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

        protected override void Update()
        {
            base.Update();
            
            _camera.orthographicSize = _gameSettings.CameraZoomModifier;
            FollowTarget();
        }

        private void FollowTarget()
        {
            if(_toFollow == false || _target == null) return;
            
            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * _gameSettings.CameraFollowSpeed);
        }
    }
}