using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class AtomicCartInstaller : MonoInstaller
    {
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private MapsContainer _mapsContainer;
        [SerializeField] private Curtains _curtains;

        [SerializeField] private GameStaticDataContainer _gameStaticDataContainer;

        [SerializeField] private Transform _popUpsParent;
        
        
        public override void InstallBindings()
        {
            Container.Bind<SceneLoader>().AsSingle().NonLazy();
            Container.Bind<PopUpService>().AsSingle().WithArguments(_popUpsParent).NonLazy();
            
            Container.BindInterfacesAndSelfTo<AtomicLogger>().AsSingle().NonLazy();
            
            Container.Bind<InternetChecker>().AsSingle().NonLazy();
            
            Container.Bind<SaveLoadService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AuthService>().AsSingle().NonLazy();
            
            Container.Bind<GameStaticDataContainer>().FromInstance(_gameStaticDataContainer).AsSingle();
            
            Container.Bind<Curtains>().FromInstance(_curtains).AsSingle();
            Container.Bind<GameSettingProvider>().AsSingle().NonLazy();
            Container.Bind<MapsContainer>().FromInstance(_mapsContainer);
            Container.Bind<GameSettings>().FromInstance(_gameSettings).AsSingle();

            Container.Bind<DiInjecter>().AsSingle().NonLazy();
        }
    }
}