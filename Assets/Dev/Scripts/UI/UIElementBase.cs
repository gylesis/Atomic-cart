using Dev.Infrastructure;
using UnityEngine;

namespace Dev.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIElementBase : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private SelectionInfoContainer _selectionInfoContainer;
                
        [SerializeField] protected DefaultReactiveButton _reactiveButton;

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _reactiveButton = GetComponentInChildren<DefaultReactiveButton>();
            _selectionInfoContainer = GetComponentInChildren<SelectionInfoContainer>();
        }

        public virtual void SetSelectionState(bool isSelected)
        {
            _selectionInfoContainer.OnParent.SetActive(isSelected);
            _selectionInfoContainer.OffParent.SetActive(!isSelected);
        }

        public virtual void SetEnableState(bool isOn)
        {
            if (isOn)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
            else
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }
        
    }
}