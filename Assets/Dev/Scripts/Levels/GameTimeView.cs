using Dev.Infrastructure;
using TMPro;
using UnityEngine;

namespace Dev
{
    public class GameTimeView : NetworkContext
    {
        [SerializeField] private TMP_Text _timeText;

        public void RPC_UpdateTime(TimeTickEventContext tickEventContext)
        {
            _timeText.text = $"{tickEventContext.LeftMinutes} : {tickEventContext.LeftSeconds} ";
        }
        
    }
}