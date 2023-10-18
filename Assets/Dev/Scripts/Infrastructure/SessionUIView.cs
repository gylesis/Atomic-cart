using TMPro;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class SessionUIView : UIElement<SessionUIView>
    {
        [SerializeField] private TMP_Text _sessionNameText;
        [SerializeField] private TMP_Text _sessionMapNameText;
        [SerializeField] private TMP_Text _sessionMapTypeText;
        [SerializeField] private TMP_Text _playerCount;
        
        public int Id { get; private set; }
        
        public void UpdateInfo(SessionGameInfo sessionGameInfo)
        {
            Id = sessionGameInfo.Id;
            _sessionNameText.text = sessionGameInfo.SessionName;
            _sessionMapNameText.text = sessionGameInfo.MapName;
            _sessionMapTypeText.text = sessionGameInfo.MapType.ToString();
            _playerCount.text = $"{sessionGameInfo.CurrentPlayers} / {sessionGameInfo.MaxPlayers}";
        }
        
    }
}