using Dev.UI;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure.Installers
{
    public class LobbySceneInstaller : MonoInstaller
    {
        [SerializeField] private SessionController _sessionController;
        
        public override void InstallBindings()
        {
            Container.Bind<SessionController>().FromInstance(_sessionController).AsSingle();
        }
    }
}