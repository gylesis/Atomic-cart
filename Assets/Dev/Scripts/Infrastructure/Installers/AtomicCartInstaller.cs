using Dev.Infrastructure.Lobby;
using Dev.Sounds;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using UnityEngine;
using Zenject;
using LogType = Fusion.LogType;

namespace Dev.Infrastructure.Installers
{
    public class AtomicCartInstaller : MonoInstaller
    {
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private MapsContainer _mapsContainer;
        [SerializeField] private Curtains _curtains;
        [SerializeField] private SoundStaticDataContainer _soundStaticDataContainer;
        
        [SerializeField] private Transform _popUpsParent;
        
        
        public override void InstallBindings()
        {
            Fusion.Log.LogLevel = (LogType)UnityEngine.LogType.Error;

            Container.Bind<GlobalDisposable>().AsSingle().NonLazy();

            Container.Bind<SceneLoader>().AsSingle().NonLazy();
            Container.Bind<PopUpService>().AsSingle().WithArguments(_popUpsParent).NonLazy();

            Container.Bind<SoundStaticDataContainer>().FromInstance(_soundStaticDataContainer).AsSingle().WhenInjectedInto<SoundController>();
            Container.Bind<SoundSettings>().AsSingle().WhenInjectedInto<SoundController>();

            Container.BindInterfacesAndSelfTo<AtomicLogger>().AsSingle().NonLazy();
            
            Container.Bind<InternetChecker>().AsSingle().NonLazy();

            BindSaveLoadScheme();
            Container.Bind<SaveLoadService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AuthService>().AsSingle().NonLazy();
            
            Container.Bind<Curtains>().FromInstance(_curtains).AsSingle();
            Container.Bind<GameSettingsProvider>().AsSingle().NonLazy();
            Container.Bind<MapsContainer>().FromInstance(_mapsContainer);
            Container.Bind<GameSettings>().FromInstance(_gameSettings).AsSingle();

            Container.Bind<DiInjecter>().AsSingle().NonLazy();
        }

        private void BindSaveLoadScheme()
        {
            Container.Bind<SaveLoadService.ISaveLoadScheme>().To<SaveLoadService.LocalSaveLoadScheme>().AsTransient().WhenInjectedInto<SaveLoadService>();
            Container.Bind<SaveLoadService.ISaveLoadScheme>().To<SaveLoadService.CloudSaveLoadScheme>().AsTransient().WhenInjectedInto<SaveLoadService>();
        }
    }
}