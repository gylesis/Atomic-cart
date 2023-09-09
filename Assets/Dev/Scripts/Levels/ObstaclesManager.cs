using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Levels
{
    public class ObstaclesManager : NetworkContext
    {
        [SerializeField] private float _barrelsRespawnCooldown = 5f;
        
        private LevelService _levelService;
        public static ObstaclesManager Instance { get; private set; }

        private List<ObstacleHealthData> _obstacleHealthDatas = new List<ObstacleHealthData>(16);
        private GameService _gameService;
        private WorldTextProvider _worldTextProvider;

        private List<Obstacle> Obstacles => _levelService.CurrentLevel.Obstacles;

        private void Awake()
        {
            Instance = this;
        }

        [Inject]
        private void Init(LevelService levelManager, GameService gameService, WorldTextProvider worldTextProvider)
        {
            _worldTextProvider = worldTextProvider;
            _gameService = gameService;
            _levelService = levelManager;
        }

        protected override void ServerSubscriptions()
        {
            base.ServerSubscriptions();

            _gameService.GameRestarted.TakeUntilDestroy(this).Subscribe((unit => OnGameRestarted()));

            foreach (Obstacle obstacle in Obstacles)
            {
                if (obstacle is ObstacleWithHealth obstacleWithHealth)
                {
                    var obstacleHealthData = new ObstacleHealthData();
                    
                    obstacleHealthData.ObstacleId = obstacle.GetInstanceID();
                    obstacleHealthData.CurrentHealth = obstacleWithHealth.Health;
                    obstacleHealthData.MaxHealth = obstacleWithHealth.Health;
                 
                    _obstacleHealthDatas.Add(obstacleHealthData);
                }
            }
        }

        private void OnGameRestarted()
        {
            foreach (Obstacle obstacle in Obstacles)
            {
                if (obstacle is ObstacleWithHealth obstacleWithHealth)
                {
                    RestoreObstacle(obstacleWithHealth);
                }
            }
        }

        private void RestoreObstacle(ObstacleWithHealth obstacleWithHealth)
        {
            obstacleWithHealth.Restore();

            ObstacleHealthData obstacleHealthData =
                _obstacleHealthDatas.First(x => x.ObstacleId == obstacleWithHealth.GetInstanceID());
            int index = _obstacleHealthDatas.IndexOf(obstacleHealthData);

            obstacleHealthData.CurrentHealth = obstacleHealthData.MaxHealth;
            _obstacleHealthDatas[index] = obstacleHealthData;
        }
        
        
        public void ApplyDamageToObstacle(PlayerRef shooter, ObstacleWithHealth obstacle, int damage)
        {
            int id = obstacle.GetInstanceID();

            ObstacleHealthData obstacleHealthData = _obstacleHealthDatas.First(x => x.ObstacleId == id);
            int indexOf = _obstacleHealthDatas.IndexOf(obstacleHealthData);

            RPC_SpawnDamageHint(shooter, obstacle.transform.position, damage);

            var currentHealth = obstacleHealthData.CurrentHealth;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnObstacleZeroHealth(obstacle);
            }

            obstacleHealthData.CurrentHealth = currentHealth;

            _obstacleHealthDatas[indexOf] = obstacleHealthData;
        }

        [Rpc]
        private void RPC_SpawnDamageHint([RpcTarget] PlayerRef playerRef, Vector3 pos, int damage)
        {
            _worldTextProvider.SpawnDamageText(pos, damage);
        }

        private void OnObstacleZeroHealth(ObstacleWithHealth obstacle)
        {
            obstacle.OnZeroHealth();

            Observable.Timer(TimeSpan.FromSeconds(_barrelsRespawnCooldown)).Subscribe((l =>
            {
                RestoreObstacle(obstacle);
            }));
        }
    }

    public struct ObstacleHealthData
    {
        public int ObstacleId;
        public int CurrentHealth;
        public int MaxHealth;
    }
}