using Dev.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI.PopUpsAndMenus
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

            _linkButton.Clicked.Subscribe((unit => OnLinkButtonClicked())).AddTo(this);
        }

        private void OnLinkButtonClicked()
        {
            if (CheckForUsername() == false)
            {
                AtomicLogger.Err("Wrong credentials");
                return;
            }
            
            PopUpService.HidePopUp<LinkProfilePopUp>();
            
            _authService.LinkWithUsernameAndPassword(_usernameInputField.text, _passwordInputField.text);
        }

        private bool CheckForUsername()
        {
            return true;
        }
    }
}