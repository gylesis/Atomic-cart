using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Levels
{
    public class ObstaclesManager : NetworkContext
    {
        private LevelService _levelService;

        private GameStateService _gameStateService;
        private HealthObjectsService _healthObjectsService;

        private List<Obstacle> Obstacles => _levelService.CurrentLevel.Obstacles;

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
            
            _levelService.LevelLoaded.Subscribe(OnLevelLoaded).AddTo(GlobalDisposable.SceneScopeToken);
            _gameStateService.GameRestarted.Subscribe(unit => OnGameRestarted()).AddTo(GlobalDisposable.SceneScopeToken);
        }

        private void OnLevelLoaded(Level level)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            foreach (Obstacle obstacle in Obstacles)
            {
                if (obstacle is ObstacleWithHealth obstacleWithHealth) 
                    _healthObjectsService.RegisterObject(obstacleWithHealth.Object.Id, obstacleWithHealth.Health);
            }
        }

        private void OnGameRestarted()
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            foreach (Obstacle obstacle in Obstacles)
            {
                if (obstacle is ObstacleWithHealth obstacleWithHealth) 
                    _healthObjectsService.RestoreObstacle(obstacleWithHealth);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if(runner.IsSharedModeMasterClient || _levelService.CurrentLevel == null) return;

            foreach (var obstacle in _levelService.CurrentLevel.Obstacles)
            {
                if (obstacle is ObstacleWithHealth) 
                    _healthObjectsService.UnRegisterObject(obstacle.Object.Id);
            }
        }
    }

}