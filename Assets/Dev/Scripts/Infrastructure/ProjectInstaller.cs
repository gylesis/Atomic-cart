using Dev.PlayerLogic;
using Dev.Utils;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private CharactersDataContainer _charactersDataContainer;
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private MapsContainer _mapsContainer;
        
        public override void InstallBindings()
        {
            Container.Bind<GameSettingProvider>().AsSingle().NonLazy();
            Container.Bind<MapsContainer>().FromInstance(_mapsContainer);
            Container.Bind<GameSettings>().FromInstance(_gameSettings).AsSingle();
            Container.Bind<CharactersDataContainer>().FromInstance(_charactersDataContainer).AsSingle();
        }
    }
}