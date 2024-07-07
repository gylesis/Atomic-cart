using Dev.Infrastructure;
using TMPro;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _killsCountText;
        [SerializeField] private TMP_Text _deathsCountText;
        
        public SessionPlayer SessionPlayer { get; private set; }
        public int Kills { get; private set; }
        public int Deaths { get; private set; }
     
        public void Init(SessionPlayer sessionPlayer, int kills, int deaths)
        {
            Kills = kills;
            Deaths = deaths;
            SessionPlayer = sessionPlayer;

            UpdateText();
        }

        public void UpdateData(int kills, int deaths)
        {
            Kills = kills;
            Deaths = deaths;

            UpdateText();
        }

        private void UpdateText()
        {
            _playerName.text = $"{SessionPlayer.Name}";
            _killsCountText.text = $"{Kills}";
            _deathsCountText.text = $"{Deaths}";
        }
        
    }
}