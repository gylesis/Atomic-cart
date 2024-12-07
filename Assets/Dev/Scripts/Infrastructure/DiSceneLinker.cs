using Zenject;

namespace Dev.Infrastructure
{
    public class DiSceneLinker
    {
        public DiSceneLinker(DiContainer sceneDi, DiInjecter diInjecter)
        {
            diInjecter.LoadSceneDiContainer(sceneDi);                        
        }
    }
}