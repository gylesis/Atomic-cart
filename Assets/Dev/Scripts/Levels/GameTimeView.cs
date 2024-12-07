using System;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Fusion;
using TMPro;
using UnityEngine;

namespace Dev.Levels
{
    public class GameTimeView : NetworkContext
    {
        [SerializeField] private TMP_Text _timeText;

        [Rpc]
        public void RPC_UpdateTime(TimeTickEventContext tickEventContext)
        {
            var timeSpan = new TimeSpan(0, 0, tickEventContext.LeftMinutes, tickEventContext.LeftSeconds);

            string minutes = timeSpan.ToString("mm");
            string seconds = timeSpan.ToString("ss");

            string timeTextText = $"{minutes} : {seconds}";
            
            _timeText.text = timeTextText;
        }
        
    }
}