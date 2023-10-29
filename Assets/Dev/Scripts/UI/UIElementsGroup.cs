using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.UI
{
    public class UIElementsGroup : MonoBehaviour
    {
        [SerializeField] private List<UIElementBase> _uiElements;
        [SerializeField] private Transform _parent;

        public List<UIElementBase> UIElements => _uiElements;

        public Transform Parent => _parent;

        private void Reset()
        {
            _parent = transform;
        }

        private void Awake()
        {
            if (_uiElements.Count == 0)
            {
                _uiElements = GetComponentsInChildren<UIElementBase>().ToList();
            }
        }

        /// <summary>
        /// Optional. Elements can be initialized from inspector or auto-get from childrens
        /// </summary>
        /// <param name="uiElementBases"></param>
        public void Init(List<UIElementBase> uiElementBases)
        {
            _uiElements = uiElementBases;
        }

        public void RemoveElement(UIElementBase elementBase)
        {
            _uiElements.Remove(elementBase);
        }

        public void AddElement(UIElementBase elementBase)
        {
            _uiElements.Add(elementBase);
        }

        public void Select(UIElementBase targetUIElement)
        {
            var exists = _uiElements.Exists(x => targetUIElement == x);

            if (exists)
            {
                foreach (UIElementBase uiElement in _uiElements)
                {
                    uiElement.SetSelectionState(targetUIElement == uiElement);
                }
            }
            else
            {
                Debug.LogError($"Such element not included in list, update UIElements list", targetUIElement);
            }
        }
        
        
    }
}