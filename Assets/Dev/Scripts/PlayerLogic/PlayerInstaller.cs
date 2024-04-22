using Dev.Infrastructure;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
        }
    }
}