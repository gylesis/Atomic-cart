using Dev.Infrastructure;
using TMPro;
using UniRx;
using Unity.Services.Authentication;
using UnityEngine;
using Zenject;

namespace Dev.UI.PopUpsAndMenus.Lobby
{
    public class ProfileSettingsMenu : PopUp
    {
        [SerializeField] private TMP_InputField _nickname;
        [SerializeField] private DefaultReactiveButton _changeNicknameButton;
        [SerializeField] private TextReactiveButton _loadLinkedAccButton;
        [SerializeField] private TextReactiveButton _linkAccountButton;

        private AuthService _authService;
        private string _prevNickname;

        protected override void Awake()
        {
            base.Awake();

            _linkAccountButton.Clicked.Subscribe(OnLinkAccountClicked).AddTo(this);
            //_loadLinkedAccButton.Clicked.Subscribe(unit => OnLoadLinkedAccClicked()).AddTo(this);
            
            _changeNicknameButton.Clicked.Subscribe(unit => OnChangeNicknameClicked()).AddTo(this);
            _nickname.onValueChanged.AsObservable().Subscribe(OnNicknameChanged).AddTo(this);
        }
        
        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }

        public override void Show()
        {
            string currentNickname = _authService.MyProfile.Nickname;
            
            _prevNickname = currentNickname;
            _nickname.text = $"{currentNickname}";
            
            OnNicknameChanged(currentNickname);
            
            _linkAccountButton.SetText(_authService.IsCurrentAccountLinkedToSomething ? "Linked!" : "Link account");
            _linkAccountButton.IsInteractable(!_authService.IsCurrentAccountLinkedToSomething);
            
            base.Show();
        }
        
        private void OnNicknameChanged(string nickname)
        {
            _changeNicknameButton.IsInteractable(nickname.Length > 0 && _prevNickname != nickname);
        }

        private async void OnChangeNicknameClicked()
        {
            Curtains.Instance.SetText("Saving nickname");
            Curtains.Instance.ShowWithDotAnimation(0);

            string newNickname = _nickname.text;
            var updateNickname = await _authService.UpdateNickname(newNickname);

            string errorMsg = GameSettingsProvider.GameSettings.IsDebugMode ? $"{updateNickname.ErrorMessage}" : "Error happened";
             
            Curtains.Instance.SetText(updateNickname.IsSuccess ? $"Nickname changed to {newNickname}" : $"{errorMsg}");
            Curtains.Instance.HideWithDelay(2f,0);
        }
        
        private void OnLoadLinkedAccClicked()
        {
            
        }

        
        private void OnLinkAccountClicked(Unit unit)
        {
            PopUpService.ShowPopUp<LinkProfilePopUp>((() =>
            {
                PopUpService.HidePopUp<LinkProfilePopUp>();
            }));
        }
    }
}