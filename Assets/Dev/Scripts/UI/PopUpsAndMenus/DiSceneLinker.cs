using Zenject;

namespace Dev.UI.PopUpsAndMenus
{
    public class DiSceneLinker
    {
        public DiSceneLinker(DiContainer sceneDi, DiInjecter diInjecter)
        {
            diInjecter.LoadSceneDiContainer(sceneDi);                        
        }
    }
}