using System;
using Dev.PlayerLogic;
using Dev.Weapons;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    public class HUDMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _showTab;
        [SerializeField] private DefaultReactiveButton _exitMenuButton;

        [SerializeField] private DefaultReactiveButton _interactionButton;
        [SerializeField] private DefaultReactiveButton _castAbilityButton;

        public Subject<Unit> CastButtonClicked { get; } = new Subject<Unit>();
        public Subject<Unit> InteractiveButtonClicked { get; } = new Subject<Unit>();

        protected override void Awake()
        {
            base.Awake();
            
            _showTab.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnShowTabButtonClicked()));
            _exitMenuButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitMenuButtonClicked()));
            _interactionButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnInteractionButtonClicked()));
            _castAbilityButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnCastButtonClicked()));
        }

        private void OnCastButtonClicked()
        {
            CastButtonClicked.OnNext(Unit.Default);
            
            NetworkRunner runner = FindObjectOfType<NetworkRunner>();
            NetworkObject playerObj = runner.GetPlayerObject(runner.LocalPlayer);

            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            AbilityCastController castController = playerObj.GetComponent<AbilityCastController>();
            
            castController.CastAbility(AbilityType.Turret, playerObj.transform.position + (Vector3)playerController.LastLookDirection * 6);
        }

        public void SetInteractionButtonState(bool enabled)
        {
            if (enabled)
            {
                _interactionButton.Disable();
            }
            else
            {
                _interactionButton.Enable();
            }
        }

        private void OnInteractionButtonClicked()
        {
            InteractiveButtonClicked.OnNext(Unit.Default);
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