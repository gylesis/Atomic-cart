using Dev.UI.PopUpsAndMenus;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class LobbySceneInstaller : MonoInstaller
    {
        [SerializeField] private PopUpService _popUpService;
        [SerializeField] private SceneLoader _sceneLoader;
        
        public override void InstallBindings()
        {
            Container.Bind<SceneLoader>().FromInstance(_sceneLoader).AsSingle();
            Container.Bind<PopUpService>().FromInstance(_popUpService).AsSingle();
        }
    }
}