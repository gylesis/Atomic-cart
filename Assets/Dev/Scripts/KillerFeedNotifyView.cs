using TMPro;
using UnityEngine;

namespace Dev
{
    public class KillerFeedNotifyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _killerText;
        [SerializeField] private TMP_Text _victimText;

        public void Setup(string killerNick, string victimText)
        {
            _killerText.text = killerNick;
            _victimText.text = victimText;
        }
    }
}