using TMPro;
using UnityEngine;

namespace Dev.UI
{
    public class TextReactiveButton : DefaultReactiveButton
    {
        [SerializeField] private TMP_Text _text;

        public void SetText(string text)
        {
            _text.text = text;
        }
        
    }
    
}