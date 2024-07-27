using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    public class LobbySettingsMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _showStatsButton;

        protected override void Awake()
        {
            base.Awake();

            _showStatsButton.Clicked.Subscribe((unit => OnShowStatButtonClicked())).AddTo(this);
        }

        private void OnShowStatButtonClicked()
        {
            PopUpService.TryGetPopUp<PlayerStatsMenu>(out var popUp);
            
            PopUpService.ShowPopUp<PlayerStatsMenu>();
            PopUpService.HidePopUp<LobbySettingsMenu>();
            
            popUp.OnSucceedButtonClicked((() =>
            {
                PopUpService.HidePopUp<PlayerStatsMenu>();
                PopUpService.ShowPopUp<LobbySettingsMenu>();
            }));
        }
    }
}   