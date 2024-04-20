using Dev.PlayerLogic;
using Dev.Utils;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class AtomicCartInstaller : MonoInstaller
    {
        [SerializeField] private CharactersDataContainer _charactersDataContainer;
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private MapsContainer _mapsContainer;
        [SerializeField] private Curtains _curtains;
        [SerializeField] private LoggerUI _loggerUI;
        
        public override void InstallBindings()
        {
            Container.Bind<LoggerUI>().FromInstance(_loggerUI).AsSingle();
            
            Container.Bind<Curtains>().FromInstance(_curtains).AsSingle();
            Container.Bind<GameSettingProvider>().AsSingle().NonLazy();
            Container.Bind<MapsContainer>().FromInstance(_mapsContainer);
            Container.Bind<GameSettings>().FromInstance(_gameSettings).AsSingle();
            Container.Bind<CharactersDataContainer>().FromInstance(_charactersDataContainer).AsSingle();
        }
    }
}