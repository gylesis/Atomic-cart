using Dev.BotsLogic;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Dev.Utils;
using Dev.Weapons.Commands;
using Dev.Weapons.Guns;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Dev.Infrastructure.Installers
{
    public class MainSceneInstaller : MonoInstaller
    {
        [SerializeField] private PlayersSpawner _playersSpawner;
        [SerializeField] private PlayersDataService _playersDataService;
        [SerializeField] private TeamsService _teamsService;
        [SerializeField] private TeamsScoreService _teamsScoreService;

        [SerializeField] private WorldTextProvider _worldTextProvider;
        [SerializeField] private TimeService _timeService;
        [FormerlySerializedAs("_gameService")] [SerializeField] private GameStateService gameStateService;

        [SerializeField] private CameraService _cameraService;
        
        [SerializeField] private LevelService _levelService;
        
        [SerializeField] private JoysticksContainer _joysticksContainer;

        [SerializeField] private BotsController _botsController;

        [SerializeField] private HealthObjectsService _healthObjectsService;
        [SerializeField] private HitsProcessor _hitsProcessor;

        [SerializeField] private AirStrikeController _airStrikeController;
        [SerializeField] private TearGasService _tearGasService;

        [SerializeField] private SessionStateService _sessionStateService;

        [SerializeField] private PlayersScoreService _playersScoreService;
        [SerializeField] private KillerFeedNotifyService _killerFeedNotifyService;
        
        public override void InstallBindings()
        {
            Container.Bind<NetworkRunner>().FromInstance(FindObjectOfType<NetworkRunner>()).AsSingle();
            Container.Bind<KillerFeedNotifyService>().FromInstance(_killerFeedNotifyService).AsSingle();
            
            Container.Bind<JoysticksContainer>().FromInstance(_joysticksContainer).AsSingle();
            
            Container.Bind<HealthObjectsService>().FromInstance(_healthObjectsService).AsSingle();
            Container.Bind<HitsProcessor>().FromInstance(_hitsProcessor).AsSingle();
            
            Container.Bind<AirStrikeController>().FromInstance(_airStrikeController).AsSingle();
            Container.Bind<TearGasService>().FromInstance(_tearGasService).AsSingle();

            Container.Bind<BotsController>().FromInstance(_botsController).AsSingle();

            Container.Bind<WorldTextProvider>().FromInstance(_worldTextProvider).AsSingle();

            Container.Bind<SessionStateService>().FromInstance(_sessionStateService).AsSingle();
            Container.Bind<PlayersScoreService>().FromInstance(_playersScoreService).AsSingle();
            
            BindPlayer();
            BindServices();
        }

        private void BindPlayer()
        {
            Container.Bind<PlayerCharacterClassChangeService>().AsSingle().NonLazy();
            Container.Bind<PlayersSpawner>().FromInstance(_playersSpawner).AsSingle();
            Container.Bind<PlayersDataService>().FromInstance(_playersDataService).AsSingle();
        }

        private void BindServices()
        {
            Container.Bind<GameStateService>().FromInstance(gameStateService).AsSingle();
            Container.Bind<LevelService>().FromInstance(_levelService).AsSingle();
            Container.Bind<CameraService>().FromInstance(_cameraService).AsSingle();
            Container.Bind<TimeService>().FromInstance(_timeService).AsSingle();
            Container.Bind<TeamsService>().FromInstance(_teamsService).AsSingle();
            Container.Bind<TeamsScoreService>().FromInstance(_teamsScoreService).AsSingle();
        }
    }
}