using Dev.Infrastructure;
using Dev.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI.PopUpsAndMenus.Lobby
{
    public class LinkProfilePopUp : PopUp
    {
        [SerializeField] private TextReactiveButton _linkButton;

        [SerializeField] private TMP_InputField _usernameInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        
        private AuthService _authService;

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }

        protected override void Awake()
        {
            base.Awake();
            _linkButton.Clicked.Subscribe(unit => OnLinkButtonClicked()).AddTo(this);

            _usernameInputField.onValueChanged.AsObservable().Subscribe(s => UpdateButtonAvailability());
            _passwordInputField.onValueChanged.AsObservable().Subscribe(s => UpdateButtonAvailability());
        }

        public override void Show()
        {
            _usernameInputField.text = _authService.MyProfile.Nickname;

            UpdateButtonAvailability();
            base.Show();
        }

        private async void OnLinkButtonClicked()
        {
            _linkButton.IsInteractable(false);
            
            if (CheckForUsername() == false)
            {
                AtomicLogger.Err("Wrong credentials");
                return;
            }

            var linkStatus = await _authService.LinkWithUsernameAndPasswordAsync(_usernameInputField.text, _passwordInputField.text);

            if (!linkStatus)
            {
                Curtains.Instance.Show();
                Curtains.Instance.SetText($"{linkStatus.ErrorMessage}");
                Curtains.Instance.HideWithDelay(1);
                
                return;
            }
            
            PopUpService.HidePopUp<LinkProfilePopUp>();

            Curtains.Instance.Show();
            Curtains.Instance.SetText($"Successfuly linked account to {_usernameInputField.text}");
            Curtains.Instance.HideWithDelay(2);

            UpdateButtonAvailability();
        }

        private void UpdateButtonAvailability()
        {
            _linkButton.IsInteractable(CheckForUsername());
        }
        
        private bool CheckForUsername()
        {
            return !string.IsNullOrEmpty(_usernameInputField.text) && !string.IsNullOrEmpty(_passwordInputField.text);
        }
    }
}