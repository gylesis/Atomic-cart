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

        [Networked, Capacity(32)] private NetworkLinkedList<ObstacleHealthData> ObstacleHealthDatas { get; }
        
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
            LevelService.Instance.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));

            base.ServerSubscriptions();

            _gameService.GameRestarted.TakeUntilDestroy(this).Subscribe((unit => OnGameRestarted()));
        }

        private void OnLevelLoaded(Level level)
        {
            foreach (Obstacle obstacle in Obstacles)
            {
                if (obstacle is ObstacleWithHealth obstacleWithHealth)
                {
                    var obstacleHealthData = new ObstacleHealthData();

                    obstacleHealthData.ObstacleId = (int) obstacle.Object.Id.Raw;
                    obstacleHealthData.CurrentHealth = obstacleWithHealth.Health;
                    obstacleHealthData.MaxHealth = obstacleWithHealth.Health;

                    ObstacleHealthDatas.Add(obstacleHealthData);
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
            int id = (int) obstacleWithHealth.Object.Id.Raw;
            
            obstacleWithHealth.Restore();

            ObstacleHealthData obstacleHealthData =
                ObstacleHealthDatas.First(x => x.ObstacleId == id);
            
            int index = ObstacleHealthDatas.IndexOf(obstacleHealthData);

            obstacleHealthData.CurrentHealth = obstacleHealthData.MaxHealth;

            ObstacleHealthDatas.Set(index, obstacleHealthData);
        }


        public void ApplyDamageToObstacle(PlayerRef shooter, ObstacleWithHealth obstacle, int damage)
        {
            int id = (int) obstacle.Object.Id.Raw;

            ObstacleHealthData obstacleHealthData = ObstacleHealthDatas.First(x => x.ObstacleId == id);
            int indexOf = ObstacleHealthDatas.IndexOf(obstacleHealthData);

            RPC_SpawnDamageHint(shooter, obstacle.transform.position, damage);

            var currentHealth = obstacleHealthData.CurrentHealth;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnObstacleZeroHealth(obstacle);
            }

            obstacleHealthData.CurrentHealth = currentHealth;

            ObstacleHealthDatas.Set(indexOf, obstacleHealthData);
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

    public struct ObstacleHealthData : INetworkStruct
    {
        [Networked] public int ObstacleId { get; set; }
        [Networked] public int CurrentHealth { get; set; }
        [Networked] public int MaxHealth { get; set; }
    }
}