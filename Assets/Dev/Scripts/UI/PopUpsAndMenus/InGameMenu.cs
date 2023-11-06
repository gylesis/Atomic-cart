using System;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace Dev.UI.PopUpsAndMenus
{
    public class InGameMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _exitButton;
        [SerializeField] private TextReactiveButton _changeCharacterClassButton;
        private PlayerCharacterClassChangeService _playerCharacterClassChangeService;
        private NetworkRunner _networkRunner;


        protected override void Awake()
        {
            base.Awake();

            _exitButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitButtonClicked()));
            _changeCharacterClassButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnChangeCharacterClassButtonClicked()));
        }

        [Inject]
        private void Init(PlayerCharacterClassChangeService playerCharacterClassChangeService, NetworkRunner networkRunner)
        {
            _networkRunner = networkRunner;
            _playerCharacterClassChangeService = playerCharacterClassChangeService;
        }
        

        private void OnChangeCharacterClassButtonClicked()
        {
            PopUpService.HidePopUp<InGameMenu>();

            PopUpService.TryGetPopUp<CharacterChooseMenu>(out var characterChooseMenu);
            
            PopUpService.ClosePrevPopUps();
            
            characterChooseMenu.StartChoosingCharacter((characterClass =>
            {
                _playerCharacterClassChangeService.ChangePlayerCharacterClass(_networkRunner.LocalPlayer, characterClass);
                PopUpService.HidePopUp<CharacterChooseMenu>();
                PopUpService.ShowPopUp<HUDMenu>();
            } ));
            
        }

        private void OnExitButtonClicked()
        {
            PopUpService.TryGetPopUp<DecidePopUp>(out var decidePopUp);

            decidePopUp.Show();
            decidePopUp.Init("Are you sure want to exit?", OnDecide);

            void OnDecide(bool isYes)
            {
                decidePopUp.Hide();

                if (isYes)
                {
                    ConnectionManager.Instance.Disconnect();
                }
                else
                {
                    
                }
            }

        }
    }
}