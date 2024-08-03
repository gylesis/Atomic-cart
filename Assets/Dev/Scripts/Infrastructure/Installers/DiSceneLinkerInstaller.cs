using Dev.UI.PopUpsAndMenus;
using Zenject;

namespace Dev.Infrastructure
{
    public class DiSceneLinkerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<DiSceneLinker>().AsSingle().NonLazy();
        }
    }
}