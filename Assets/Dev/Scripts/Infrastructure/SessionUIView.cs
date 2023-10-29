using System;
using Dev.UI;
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
        [SerializeField] private TMP_Text _statusText;
        
        public int Id { get; private set; }
        
        public void UpdateInfo(SessionGameInfo sessionGameInfo)
        {
            Id = sessionGameInfo.Id;
            _sessionNameText.text = sessionGameInfo.SessionName;
            _sessionMapNameText.text = sessionGameInfo.MapName;
            _sessionMapTypeText.text = sessionGameInfo.MapType.ToString();
            _playerCount.text = $"{sessionGameInfo.CurrentPlayers} / {sessionGameInfo.MaxPlayers}";
            
            UpdateStatus(sessionGameInfo.SessionStatus);
        }

        private void UpdateStatus(SessionStatus sessionStatus)
        {
            string text = "";
            Color color;
            
            switch (sessionStatus)
            {
                case SessionStatus.Lobby:
                    color = Color.green;
                    text = "Waiting for players";
                    break;
                case SessionStatus.InGame:
                    color = Color.yellow;
                    text = "In game";
                    break;
                case SessionStatus.Starting:
                    color = Color.blue;
                    text = "Starting";
                    break;
                default:
                    color = Color.white;
                    break;
            }

            _statusText.color = color;
            _statusText.text = text;
        }
        
    }
}