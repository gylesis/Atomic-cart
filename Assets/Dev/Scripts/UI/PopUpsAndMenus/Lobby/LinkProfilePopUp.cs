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
        }

        private async void OnLinkButtonClicked()
        {
            _linkButton.Disable();
            
            if (CheckForUsername() == false)
            {
                AtomicLogger.Err("Wrong credentials");
                return;
            }
            
            PopUpService.HidePopUp<LinkProfilePopUp>();
            
            await _authService.LinkWithUsernameAndPasswordAsync(_usernameInputField.text, _passwordInputField.text);
            
            _linkButton.Enable();
        }

        private bool CheckForUsername()
        {
            return true;
        }
    }
}