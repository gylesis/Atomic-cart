using Dev.UI;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class LobbySceneInstaller : MonoInstaller
    {
        [SerializeField] private PopUpService _popUpService;
        [SerializeField] private NetworkRunner _networkRunner;
        [SerializeField] private SceneLoader _sceneLoader;
        public override void InstallBindings()
        {
            Container.Bind<SceneLoader>().FromInstance(_sceneLoader).AsSingle();
            Container.Bind<NetworkRunner>().FromInstance(_networkRunner).AsSingle();
            Container.Bind<PopUpService>().FromInstance(_popUpService).AsSingle();
        }
    }
}