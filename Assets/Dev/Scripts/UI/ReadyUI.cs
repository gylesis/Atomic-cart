using Dev.Infrastructure;
using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Dev.UI
{
    public class ReadyUI : NetworkContext
    {
        [SerializeField] private TMP_Text _playerNicknameText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _readyImage;
        private AuthService _authService;

        [Networked]
        public PlayerRef PlayerRef { get; private set; }

        private void Awake()
        {
            _canvasGroup.alpha = 0;
        }

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
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

        public async void UpdateNickname()
        {
            string nickname = await UnityDataLinker.Instance.GetNicknameAsync(PlayerRef);

            _playerNicknameText.text = $"{nickname}";
            //_playerNicknameText.text = $"moloko";
        }

        [Rpc]
        public void RPC_SetReadyView(bool isReady)
        {
            _canvasGroup.alpha = 1;
            _readyImage.color = isReady ? Color.green : Color.white  ;
        }
        
    }
}