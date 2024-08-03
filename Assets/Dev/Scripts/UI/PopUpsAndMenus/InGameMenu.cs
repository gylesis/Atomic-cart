using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
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

            var characterChooseMenu = PopUpService.ShowPopUp<CharacterChooseMenu>();

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
            var decidePopUp = PopUpService.ShowPopUp<DecidePopUp>();
            
            decidePopUp.SetTitle("Are you sure want to exit?", OnDecide);

            void OnDecide(bool isYes)
            {
                PopUpService.HidePopUp<DecidePopUp>();

                if (isYes)
                {
                    MainSceneConnectionManager.Instance.Disconnect();
                }
            }

        }
    }
}