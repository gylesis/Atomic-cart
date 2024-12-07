using Zenject;

namespace Dev.Infrastructure.Installers
{
    public class DiSceneLinkerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<DiSceneLinker>().AsSingle().NonLazy();
        }
    }
}