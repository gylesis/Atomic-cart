using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Utils;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;
using Random = UnityEngine.Random;

namespace Dev.Levels.Env
{
    public class CloudsController : MonoContext
    {
        [SerializeField] private Transform _parent;

        [SerializeField] private float _cloudsCount = 64;
        [SerializeField] private float _maxSpeed = 5;
        [SerializeField] private float _minSpeed = 1;

        [SerializeField] private float _minSize = 0.1f;
        [SerializeField] private float _maxSize = 2f;

        [SerializeField, Range(-1, 1)] private int _moveDirection = -1;
        
        private ObjectPool<Cloud> _cloudsPool;
        private HashSet<Cloud> _clouds = new HashSet<Cloud>();
        private LevelService _levelService;
        private GameSettings _gameSettings;

        protected override void Awake()
        {
            base.Awake();
            _cloudsPool = new ObjectPool<Cloud>(CreateFunc, actionOnRelease: ActionOnRelease, actionOnGet: ActionOnGet, defaultCapacity: 64);
        }

        [Inject]
        private void Construct(GameSettings gameSettings, LevelService levelService)
        {
            _gameSettings = gameSettings;
            _levelService = levelService;
        }

        protected override void OnInjectCompleted()
        {
            _levelService.LevelLoaded.Subscribe(OnLevelCreated).AddTo(GlobalDisposable.SceneScopeToken);
            OnLevelCreated(_levelService.CurrentLevel);
        }

        private void OnLevelCreated(Level level)
        {
            if(level == null) return;
            
            for (int i = 0; i < _cloudsCount; i++)
            {
                Vector3 randomPos = level.MetaData.Bounds.RandomPointInBounds();
                randomPos.z = 1;

                Cloud cloud = _cloudsPool.Get();
                cloud.transform.position = randomPos;

                float randomSpeed = Random.Range(_minSpeed, _maxSpeed);
                float randomSize = Random.Range(_minSize, _maxSize);

                cloud.Setup(randomSize, randomSpeed, _gameSettings.LevelEnvironmentConfig.CloudsConfig.CloudsSprites.GetRandom());
            }
        }


        [ContextMenu(nameof(RecreateClouds))]
        private void RecreateClouds()
        {
            _cloudsPool.Clear();
            
            foreach (var cloud in _clouds) Destroy(cloud.gameObject);
            _clouds.Clear();
            
            OnLevelCreated(_levelService.CurrentLevel);
        }

        #region Pooling
        private Cloud CreateFunc()
        {
            var instance = Instantiate(_gameSettings.LevelEnvironmentConfig.CloudsConfig.CloudPrefab, _parent);
            instance.gameObject.SetActive(false);
            
            return instance;
        }

        private void ActionOnGet(Cloud cloud)
        {
            cloud.gameObject.SetActive(true);
            _clouds.Add(cloud);
        }

        private void ActionOnRelease(Cloud cloud)
        {
            cloud.gameObject.SetActive(false);
            _clouds.Remove(cloud);
        }
        #endregion

        private void Update()
        {
            if(!LevelService.IsNetInitialized) return;
            
            Level currentLevel = _levelService.CurrentLevel;
            if(currentLevel == null) return;

            foreach (var cloud in _clouds)
            {
                if(cloud == null) return;
                
                if (!currentLevel.MetaData.Bounds.Contains(cloud.transform.position)) 
                    OnReachCorner(cloud);
                
                cloud.Move(Vector3.right * _moveDirection);
            }
        }

        private void OnReachCorner(Cloud cloud)
        {
            Level currentLevel = _levelService.CurrentLevel;

            Vector3 cloudPos = cloud.transform.position;
            float xDirection = currentLevel.MetaData.Bounds.extents.x * Mathf.Sign(_moveDirection);
                    
            Vector3 nextPos = currentLevel.MetaData.Bounds.center + Vector3.right * -xDirection;
            nextPos.y = cloudPos.y;
            nextPos.z = cloudPos.z;
                    
            cloud.transform.position = nextPos;
            
            cloud.UpdateSprite(_gameSettings.LevelEnvironmentConfig.CloudsConfig.CloudsSprites.GetRandom());
        }
    }
}