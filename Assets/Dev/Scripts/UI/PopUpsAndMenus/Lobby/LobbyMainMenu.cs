using Dev.Infrastructure;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI.PopUpsAndMenus.Lobby
{
    public class LobbyMainMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _playButton;
        [SerializeField] private DefaultReactiveButton _settingsButton;
        [SerializeField] private DefaultReactiveButton _exitButton;

        [SerializeField] private TMP_Text _nicknameText;
        private AuthService _authService;

        protected override void Awake()
        {
            _playButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnPlayButtonClicked()));
            _settingsButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnSettingsButtonClicked()));
            _exitButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitButtonClicked()));
            
            Show();
        }

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }

        private void OnPlayButtonClicked()
        {
            Hide();
            PopUpService.ShowPopUp<LobbyPlayMenu>((() =>
            {
                PopUpService.HidePopUp<LobbyPlayMenu>();
                PopUpService.ShowPopUp<LobbyMainMenu>();
            }));
        }

        private void OnSettingsButtonClicked()
        {
            PopUpService.HidePopUp<LobbyMainMenu>();
            PopUpService.ShowPopUp<SettingsMenu>((() =>
            {
                PopUpService.HidePopUp<SettingsMenu>();
                PopUpService.ShowPopUp<LobbyMainMenu>();
            }));
        }

        public override void Show()
        {
            base.Show();

            _nicknameText.text = $"{_authService.MyProfile.Nickname}";
        }

        private void OnExitButtonClicked()
        {
            Application.Quit();
        }
    }
}