using Dev.BotsLogic;
using Dev.CartLogic;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Dev.Weapons;
using Dev.Weapons.Guns;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class MainSceneInstaller : MonoInstaller
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

        [SerializeField] private BotsController _botsController;

        [SerializeField] private HealthObjectsService _healthObjectsService;
        [SerializeField] private HitsProcessor _hitsProcessor;

        [SerializeField] private AirStrikeController _airStrikeController;
        [SerializeField] private TearGasService _tearGasService;
        
        public override void InstallBindings()
        {
            Container.Bind<NetworkRunner>().FromInstance(FindObjectOfType<NetworkRunner>()).AsSingle();
            
            Container.Bind<DependenciesContainer>().AsSingle().NonLazy();

            Container.Bind<JoysticksContainer>().FromInstance(_joysticksContainer).AsSingle();
            
            Container.Bind<HealthObjectsService>().FromInstance(_healthObjectsService).AsSingle();
            Container.Bind<HitsProcessor>().FromInstance(_hitsProcessor).AsSingle();
            
            Container.Bind<AirStrikeController>().FromInstance(_airStrikeController).AsSingle();
            Container.Bind<TearGasService>().FromInstance(_tearGasService).AsSingle();

            Container.Bind<BotsController>().FromInstance(_botsController).AsSingle();

            Container.Bind<WorldTextProvider>().FromInstance(_worldTextProvider).AsSingle();
           
            BindPlayer();
            BindServices();
        }

        private void BindPlayer()
        {
            Container.Bind<PlayerCharacterClassChangeService>().AsSingle().NonLazy();
            Container.Bind<PlayersSpawner>().FromInstance(_playersSpawner).AsSingle();
            Container.Bind<PlayersHealthService>().FromInstance(_playersHealthService).AsSingle();
            Container.Bind<PlayersDataService>().FromInstance(_playersDataService).AsSingle();
        }

        private void BindServices()
        {
            Container.Bind<GameService>().FromInstance(_gameService).AsSingle();
            Container.Bind<LevelService>().FromInstance(_levelService).AsSingle();
            Container.Bind<CameraService>().FromInstance(_cameraService).AsSingle();
            Container.Bind<PopUpService>().FromInstance(_popUpService).AsSingle();
            Container.Bind<TimeService>().FromInstance(_timeService).AsSingle();
            Container.Bind<TeamsService>().FromInstance(_teamsService).AsSingle();
            Container.Bind<TeamsScoreService>().FromInstance(_teamsScoreService).AsSingle();
        }
    }
}