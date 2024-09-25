using Dev.Infrastructure;
using Zenject;

namespace Dev.BotsLogic
{
    public class BotInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindStates();
            
            Container.BindInterfacesAndSelfTo<BotStateController>().AsSingle().NonLazy();
            Container.Bind<Bot>().FromComponentOnRoot().AsSingle();
        }

        private void BindStates()
        {
            Container.Bind<IBotState>().To<PatrolBotState>().AsTransient();
            Container.Bind<IBotState>().To<AttackPlayerBotState>().AsTransient();
        }
        
    }
}