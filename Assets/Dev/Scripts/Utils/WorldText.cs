using TMPro;
using UnityEngine;

namespace Dev.Utils
{
    public class WorldText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        public void Setup(string text, Color color)
        {
            _text.color = color;
            _text.text = text;
        }
    }
}