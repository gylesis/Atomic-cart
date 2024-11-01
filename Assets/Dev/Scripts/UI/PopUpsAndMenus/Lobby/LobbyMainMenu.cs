using TMPro;
using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    public class LobbyMainMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _playButton;
        [SerializeField] private DefaultReactiveButton _settingsButton;
        [SerializeField] private DefaultReactiveButton _exitButton;

        [SerializeField] private TMP_Text _nicknameText;
        
        protected override void Awake()
        {
            _playButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnPlayButtonClicked()));
            _settingsButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnSettingsButtonClicked()));
            _exitButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitButtonClicked()));
            
            Show();
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

            _nicknameText.text = $"{SaveLoadService.Instance.Profile.Nickname}";
        }

        private void OnExitButtonClicked()
        {
            Application.Quit();
        }
    }
}