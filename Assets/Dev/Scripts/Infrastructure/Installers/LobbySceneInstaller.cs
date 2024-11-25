using Dev.UI;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
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