using Dev.Infrastructure;
using Fusion;
using TMPro;
using UnityEngine;

namespace Dev
{
    public class GameTimeView : NetworkContext
    {
        [SerializeField] private TMP_Text _timeText;

        [Rpc]
        public void RPC_UpdateTime(TimeTickEventContext tickEventContext)
        {
            _timeText.text = $"{tickEventContext.LeftMinutes} : {tickEventContext.LeftSeconds} ";
        }
        
    }
}