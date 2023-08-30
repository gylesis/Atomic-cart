using Dev.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.UI
{
    public class CharacterPickUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _classText;
        [SerializeField] private Image _characterIcon;
        [SerializeField] private TMP_Text _healthText;

        [SerializeField] private CharacterChooseReactiveButton _chooseButton;

        [SerializeField] private Transform _highlightTransform;
        
        public CharacterChooseReactiveButton ChooseButton => _chooseButton;

        public void Setup(Sprite characterIcon, CharacterClass characterClass, int health)
        {
            _chooseButton.Init(characterClass, this);
            
            _classText.text = characterClass.ToString();    
            _healthText.text = $"{health}";
            _characterIcon.sprite = characterIcon;
        }
        
        public void Highlight(bool isOn)
        {
            _highlightTransform.gameObject.SetActive(isOn);
        }
        
    }
}