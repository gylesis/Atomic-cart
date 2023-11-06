using System;
using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    public class HUDMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _showTab;
        [SerializeField] private DefaultReactiveButton _exitMenuButton;

        [SerializeField] private DefaultReactiveButton _interactionButton;
        
        private Action _onActionButtonPressed;

        protected override void Awake()
        {
            base.Awake();
            
            _showTab.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnShowTabButtonClicked()));
            _exitMenuButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitMenuButtonClicked()));
            _interactionButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnInteractionButtonClicked()));
            
            SetInteractionAction(null);
        }

        private void OnInteractionButtonClicked()
        {
            _onActionButtonPressed?.Invoke();           
        }

        public void SetInteractionAction(Action onActionButtonPressed)
        {
            _onActionButtonPressed = onActionButtonPressed;
            
            if (onActionButtonPressed == null)
            {
                _interactionButton.Disable();
            }
            else
            {
                _interactionButton.Enable();
            }
            
        }
        
        private void OnExitMenuButtonClicked()
        {
            PopUpService.TryGetPopUp<InGameMenu>(out var exitPopUp);
            
            Hide();
            exitPopUp.Show();
            
            exitPopUp.OnSucceedButtonClicked((() =>
            {
                exitPopUp.Hide();
                Show();
            }));
            
        }

        private void OnShowTabButtonClicked()
        {
            var tryGetPopUp = PopUpService.TryGetPopUp<PlayersScoreMenu>(out var playersScoreMenu);

            if (tryGetPopUp)
            {
                Hide();

                playersScoreMenu.Show();

                playersScoreMenu.OnSucceedButtonClicked((() =>
                {
                    playersScoreMenu.Hide();
                    Show();
                }));
            }
        }
    }
}