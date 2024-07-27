using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Dev
{
    public class AuthMenu : PopUp
    {
        [SerializeField] private TMP_InputField _loginInput;
        [SerializeField] private TMP_InputField _passwordInput;

        [SerializeField] private DefaultReactiveButton _loginButton;
        
        private AuthService _authService;


        protected override void Awake()
        {
            base.Awake();

            _loginInput.onSubmit.AsObservable().Subscribe((OnSubmit)).AddTo(this);

            _loginButton.Clicked.Subscribe((unit => OnLoginButtonClicked())).AddTo(this);
        }

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }

        private void OnLoginButtonClicked()
        {
        }

        private void OnSubmit(string input) 
        {
            
        }
    }
}