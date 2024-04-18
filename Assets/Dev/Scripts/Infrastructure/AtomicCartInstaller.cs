using Dev.PlayerLogic;
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
        
        public override void InstallBindings()
        {
            Container.Bind<Curtains>().FromInstance(_curtains).AsSingle();
            Container.Bind<GameSettingProvider>().AsSingle().NonLazy();
            Container.Bind<MapsContainer>().FromInstance(_mapsContainer);
            Container.Bind<GameSettings>().FromInstance(_gameSettings).AsSingle();
            Container.Bind<CharactersDataContainer>().FromInstance(_charactersDataContainer).AsSingle();
        }
    }
}