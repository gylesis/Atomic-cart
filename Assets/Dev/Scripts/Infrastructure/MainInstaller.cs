using Dev.UI;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private CharactersDataContainer _charactersDataContainer;

        [SerializeField] private PlayersSpawner _playersSpawner;
        [SerializeField] private PlayersHealthService _playersHealthService;
        [SerializeField] private PlayersDataService _playersDataService;
        
        [SerializeField] private WorldTextProvider _worldTextProvider;
        [SerializeField] private TeamsService _teamsService;
        [SerializeField] private CartPathService _cartPathService;
        [SerializeField] private PopUpService _popUpService;
        [SerializeField] private TimeService _timeService;
        
        public override void InstallBindings()
        {
            Container.Bind<DependenciesContainer>().AsSingle().NonLazy();
            
            Container.Bind<CharactersDataContainer>().FromInstance(_charactersDataContainer).AsSingle();
            
            Container.Bind<PlayersSpawner>().FromInstance(_playersSpawner).AsSingle();
            Container.Bind<PlayersHealthService>().FromInstance(_playersHealthService).AsSingle();
            Container.Bind<PlayersDataService>().FromInstance(_playersDataService).AsSingle();
            
            Container.Bind<TeamsService>().FromInstance(_teamsService).AsSingle();
            Container.Bind<WorldTextProvider>().FromInstance(_worldTextProvider).AsSingle();
            
            Container.Bind<CartPathService>().FromInstance(_cartPathService).AsSingle();
            
            Container.Bind<PopUpService>().FromInstance(_popUpService).AsSingle();
            Container.Bind<TimeService>().FromInstance(_timeService).AsSingle();
            
        }
    }
}