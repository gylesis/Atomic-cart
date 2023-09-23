using Dev.CartLogic;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Dev.Utils;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private PlayersSpawner _playersSpawner;
        [SerializeField] private PlayersHealthService _playersHealthService;
        [SerializeField] private PlayersDataService _playersDataService;
        [SerializeField] private TeamsService _teamsService;
        [SerializeField] private TeamsScoreService _teamsScoreService;

        [SerializeField] private WorldTextProvider _worldTextProvider;
        [SerializeField] private PopUpService _popUpService;
        [SerializeField] private TimeService _timeService;
        [SerializeField] private GameService _gameService;

        [SerializeField] private LevelsContainer _levelsContainer;
        
        [SerializeField] private CameraService _cameraService;
        
        [SerializeField] private LevelService _levelService;
        
        [SerializeField] private JoysticksContainer _joysticksContainer;
        
        public override void InstallBindings()  
        {
            Container.Bind<DependenciesContainer>().AsSingle().NonLazy();

            Container.Bind<JoysticksContainer>().FromInstance(_joysticksContainer).AsSingle();

            Container.Bind<GameService>().FromInstance(_gameService).AsSingle();

            Container.Bind<LevelsContainer>().FromInstance(_levelsContainer).AsSingle();
            Container.Bind<LevelService>().FromInstance(_levelService).AsSingle();

            Container.Bind<CameraService>().FromInstance(_cameraService).AsSingle();
            
            Container.Bind<PlayersSpawner>().FromInstance(_playersSpawner).AsSingle();
            Container.Bind<PlayersHealthService>().FromInstance(_playersHealthService).AsSingle();
            Container.Bind<PlayersDataService>().FromInstance(_playersDataService).AsSingle();

            Container.Bind<WorldTextProvider>().FromInstance(_worldTextProvider).AsSingle();

            Container.Bind<PopUpService>().FromInstance(_popUpService).AsSingle();
            Container.Bind<TimeService>().FromInstance(_timeService).AsSingle();
            Container.Bind<TeamsService>().FromInstance(_teamsService).AsSingle();
            Container.Bind<TeamsScoreService>().FromInstance(_teamsScoreService).AsSingle();
        }
    }
}