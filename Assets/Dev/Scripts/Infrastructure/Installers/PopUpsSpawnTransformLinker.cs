using Dev.UI.PopUpsAndMenus;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class PopUpsSpawnTransformLinker : MonoBehaviour
    {
        [SerializeField] private Transform _popUpsParent;

        [Inject]
        private void Construct(PopUpService popUpService)
        {
            if(_popUpsParent != null)
                popUpService.UpdateSceneLink(_popUpsParent);
        }
    }
}