using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    public class LobbyMainMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _playButton;
        [SerializeField] private DefaultReactiveButton _settingsButton;
        [SerializeField] private DefaultReactiveButton _exitButton;

        protected override void Awake()
        {
            _playButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnPlayButtonClicked()));
            _settingsButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnSettingsButtonClicked()));
            _exitButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitButtonClicked()));
        }

        private void OnPlayButtonClicked()
        {
            Hide();
            PopUpService.ShowPopUp<LobbyPlayMenu>();
        }

        private void OnSettingsButtonClicked()
        {
            Hide();
            PopUpService.TryGetPopUp<LobbySettingsMenu>(out var settingsMenu);
            
            settingsMenu.Show();
            settingsMenu.OnSucceedButtonClicked((() =>
            {
                settingsMenu.Hide();
                Show();
            }));
        }

        private void OnExitButtonClicked()
        {
            Application.Quit();
        }
    }
}