using Dev.UI;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class LobbySceneInstaller : MonoInstaller
    {
        [SerializeField] private PopUpService _popUpService;
        
        public override void InstallBindings()
        {
            Container.Bind<PopUpService>().FromInstance(_popUpService);
        }
    }
}