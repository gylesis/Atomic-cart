using Dev.CartLogic;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Fusion;
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

        [SerializeField] private CameraService _cameraService;
        
        [SerializeField] private LevelService _levelService;
        
        [SerializeField] private JoysticksContainer _joysticksContainer;
        
        public override void InstallBindings()
        {
            Container.Bind<NetworkRunner>().FromInstance(FindObjectOfType<NetworkRunner>()).AsSingle();
            
            Container.Bind<DependenciesContainer>().AsSingle().NonLazy();

            Container.Bind<JoysticksContainer>().FromInstance(_joysticksContainer).AsSingle();

            Container.Bind<GameService>().FromInstance(_gameService).AsSingle();

            Container.Bind<LevelService>().FromInstance(_levelService).AsSingle();

            Container.Bind<CameraService>().FromInstance(_cameraService).AsSingle();

            Container.Bind<PlayerCharacterClassChangeService>().AsSingle().NonLazy();
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