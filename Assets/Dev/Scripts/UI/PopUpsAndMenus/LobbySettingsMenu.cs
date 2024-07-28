using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    public class LobbySettingsMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _showStatsButton;
        [SerializeField] private TextReactiveButton _showProfileSettingsButton;
        
        
        protected override void Awake()
        {
            base.Awake();

            _showStatsButton.Clicked.Subscribe((unit => OnShowStatButtonClicked())).AddTo(this);
            _showProfileSettingsButton.Clicked.Subscribe((unit =>  PopUpService.ShowPopUp<ProfileSettingsMenu>((() => PopUpService.HidePopUp<ProfileSettingsMenu>())))).AddTo(this);
        }

        private void OnShowStatButtonClicked()
        {
            PopUpService.ShowPopUp<PlayerStatsMenu>();
            PopUpService.HidePopUp<LobbySettingsMenu>();
            
            PopUpService.ShowPopUp<PlayerStatsMenu>((() =>
            {
                PopUpService.HidePopUp<PlayerStatsMenu>();
                PopUpService.ShowPopUp<LobbySettingsMenu>();
            }));
           
        }
    }
}   