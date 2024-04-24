using Dev.PlayerLogic;
using Dev.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Dev.Infrastructure
{
    public class AtomicCartInstaller : MonoInstaller
    {
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private MapsContainer _mapsContainer;
        [SerializeField] private Curtains _curtains;
        [FormerlySerializedAs("_loggerUI")] [SerializeField] private MyLogger myLogger;

        [SerializeField] private GameStaticDataContainer _gameStaticDataContainer;
        
        public override void InstallBindings()
        {
            Container.Bind<MyLogger>().FromInstance(myLogger).AsSingle();

            Container.Bind<GameStaticDataContainer>().FromInstance(_gameStaticDataContainer).AsSingle();
            
            Container.Bind<Curtains>().FromInstance(_curtains).AsSingle();
            Container.Bind<GameSettingProvider>().AsSingle().NonLazy();
            Container.Bind<MapsContainer>().FromInstance(_mapsContainer);
            Container.Bind<GameSettings>().FromInstance(_gameSettings).AsSingle();
        }
    }
}