using Cysharp.Threading.Tasks;
using Dev.Infrastructure.Networking;
using Dev.PlayerLogic;
using Dev.UI.PopUpsAndMenus.Other;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI.PopUpsAndMenus.Main
{
    public class InGameMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _exitButton;
        [SerializeField] private TextReactiveButton _changeCharacterClassButton;
        [SerializeField] private DefaultReactiveButton _settingsButton;
        
        private PlayerCharacterClassChangeService _playerCharacterClassChangeService;
        private NetworkRunner _networkRunner;

        protected override void Awake()
        {
            base.Awake();

            _settingsButton.Clicked.Subscribe((unit => OnSettingsButtonClicked())).AddTo(this);
            _exitButton.Clicked.Subscribe(unit => OnExitButtonClicked().Forget()).AddTo(this);
            _changeCharacterClassButton.Clicked.Subscribe(unit => OnChangeCharacterClassButtonClicked()).AddTo(this);

            _procceedButton.Clicked.Subscribe((unit => OnResumeButtonClicked())).AddTo(this);
        }

        private void OnResumeButtonClicked()
        {
            PopUpService.HidePopUp(this);
            PopUpService.ShowPopUp<HUDMenu>();
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

        private void OnSettingsButtonClicked()
        {
            PopUpService.HidePopUp(this);
            PopUpService.ShowPopUp<SettingsMenu>((() =>
            {
                PopUpService.ShowPopUp<InGameMenu>();
                PopUpService.HidePopUp<SettingsMenu>();
            }));
        }

        private async UniTask OnExitButtonClicked()
        {
            var decidePopUp = PopUpService.ShowPopUp<DecidePopUp>();
            
            decidePopUp.SetTitle("Are you sure want to exit?");

            bool answer = await decidePopUp.WaitAnswer();

            PopUpService.HidePopUp<DecidePopUp>();

            if (answer) 
                MainSceneConnectionManager.Instance.Disconnect();
        }
    }
}