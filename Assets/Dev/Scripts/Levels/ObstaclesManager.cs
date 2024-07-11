using System.Collections.Generic;
using Dev.Infrastructure;
using UniRx;
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
            
            _levelService.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
            _gameStateService.GameRestarted.TakeUntilDestroy(this).Subscribe((unit => OnGameRestarted()));
        }

        private void OnLevelLoaded(Level level)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            foreach (Obstacle obstacle in Obstacles)
            {
                if (obstacle is ObstacleWithHealth obstacleWithHealth)
                {
                    _healthObjectsService.RegisterObject(obstacle.Object, obstacleWithHealth.Health);
                }
            }
        }

        private void OnGameRestarted()
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            foreach (Obstacle obstacle in Obstacles)
            {
                if (obstacle is ObstacleWithHealth obstacleWithHealth)
                {
                    _healthObjectsService.RestoreObstacle(obstacleWithHealth);
                }
            }
        }
    }

}