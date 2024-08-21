using System;
using Dev.Infrastructure;
using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.UI
{
    public class ReadyUI : NetworkContext
    {
        [SerializeField] private TMP_Text _playerNicknameText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _readyImage;
        
        [Networked]
        public PlayerRef PlayerRef { get; private set; }

        private void Awake()
        {
            _canvasGroup.alpha = 0;
        }

        [Rpc]
        public void RPC_AssignPlayer(PlayerRef playerRef)
        {
            PlayerRef = playerRef;

            Object.AssignInputAuthority(playerRef);
            
            _canvasGroup.alpha = 0;
            _canvasGroup.DOFade(1, 0.5f);
            
            UpdateNickname();
        }

        [Rpc]
        public void RPC_RemovePlayerAssigment()
        {
             PlayerRef = PlayerRef.None;
             
             Object.AssignInputAuthority(PlayerRef.None);

             _canvasGroup.alpha = 0;
        }

        public void UpdateNickname()
        {
            _playerNicknameText.text = $"{PlayersDataLinker.Instance.GetNickname(PlayerRef)}";
        }

        [Rpc]
        public void RPC_SetReadyView(bool isReady)
        {
            _canvasGroup.alpha = 1;
            _readyImage.color = isReady ? Color.green : Color.white  ;
        }
        
    }
}