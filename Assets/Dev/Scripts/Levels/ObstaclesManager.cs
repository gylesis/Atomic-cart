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
        private LevelService _levelService;
        public static ObstaclesManager Instance { get; private set; }

        [Networked, Capacity(32)] private NetworkLinkedList<ObstacleHealthData> ObstacleHealthDatas { get; }
        
        private GameStateService _gameStateService;
        private HealthObjectsService _healthObjectsService;

        private List<Obstacle> Obstacles => _levelService.CurrentLevel.Obstacles;

        private void Awake()
        {
            Instance = this;
        }

        [Inject]
        private void Init(LevelService levelManager, GameStateService gameStateService, HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
            _gameStateService = gameStateService;
            _levelService = levelManager;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _levelService.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
            _gameStateService.GameRestarted.TakeUntilDestroy(this).Subscribe((unit => OnGameRestarted()));
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
                    _healthObjectsService.RestoreObstacle(obstacleWithHealth);
                }
            }
        }
    }

    public struct ObstacleHealthData : INetworkStruct
    {
        [Networked] public int ObstacleId { get; set; }
        [Networked] public int CurrentHealth { get; set; }
        [Networked] public int MaxHealth { get; set; }
    }
}