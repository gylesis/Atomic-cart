using Dev.Infrastructure;
using Fusion;
using TMPro;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerScoreUI : NetworkContext
    {
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _killsCountText;
        [SerializeField] private TMP_Text _deathsCountText;
        
        [Networked]
        public PlayerRef PlayerId { get; private set; }

        
        [Rpc]
        public void RPC_Init(string nickname, int kills, int deaths, PlayerRef playerId)
        {
            PlayerId = playerId;
            _playerName.text = nickname;
            _killsCountText.text = $"{kills}";
            _deathsCountText.text = $"{deaths}";
        }

        [Rpc]
        public void RPC_InitNickname(string nickname) // TODO replace
        {
            _playerName.text = nickname;
        }
        
        [Rpc]
        public void RPC_UpdateData(int kills, int deaths)
        {
            _killsCountText.text = $"{kills}";
            _deathsCountText.text = $"{deaths}";
        }
        
    }
}