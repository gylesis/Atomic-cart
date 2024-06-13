using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Weapons;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI.PopUpsAndMenus
{
    public class HUDMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _showTab;
        [SerializeField] private DefaultReactiveButton _exitMenuButton;

        [SerializeField] private DefaultReactiveButton _interactionButton;
        [SerializeField] private LongClickReactiveButton _castAbilityButton;

        private PlayerCharacter _playerCharacter;
        private AbilityCastController _castController;
        private PlayersSpawner _playersSpawner;

        public Subject<Unit> CastButtonClicked { get; } = new Subject<Unit>();
        public Subject<Unit> InteractiveButtonClicked { get; } = new Subject<Unit>();

        protected override void Awake()
        {
            base.Awake();

            _showTab.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnShowTabButtonClicked()));
            _exitMenuButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitMenuButtonClicked()));
            _interactionButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnInteractionButtonClicked()));

            _castAbilityButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnCastButtonClicked()));
            _castAbilityButton.LongClick.TakeUntilDestroy(this)
                .Subscribe((unit => OnCastButtonLongClicked()));
        }

        [Inject]
        private void Construct(PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
        }

        private void Start()
        {
            _playersSpawner.PlayerSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext context)
        {
            if (_playerCharacter != null) return;

            NetworkRunner runner = FindObjectOfType<NetworkRunner>();

            if (runner.LocalPlayer != context.PlayerRef) return;

            _playerCharacter = context.Transform.GetComponent<PlayerCharacter>();

            _castAbilityButton.SetAllowToLongClick(false);
            
            _castController = _playerCharacter.GetComponent<AbilityCastController>();
            _castController.AbilityRecharged.TakeUntilDestroy(this).Subscribe((OnAbilityRecharged));
        }

        private void OnCastButtonClicked()
        {
            Debug.Log($"Cast clicked {_castController.AllowToCast}");
            
            if(_castController.AllowToCast == false) return;
            
            if (_castController.CurrentAbilityToCast == AbilityType.MiniAirStrike)
            {
                _castAbilityButton.SetAllowToLongClick(false);
            }
            else
            {
                _castAbilityButton.SetAllowToLongClick(true);
            }
            
            CastButtonClicked.OnNext(Unit.Default);

            PlayerInput input = new PlayerInput();
            input.CastAbility = true;
            _playerCharacter.InputService.SimulateInput(input);
        }

        private void OnCastButtonLongClicked()
        {
            _castAbilityButton.SetAllowToLongClick(false);
            
            if (_castController.CurrentAbilityToCast == AbilityType.MiniAirStrike)
            {
                return;
            }

            _castAbilityButton.ResetProgressImage();
            _castController.ResetAbility();
        }

        private void OnAbilityRecharged(AbilityType abilityType)
        {
            if (abilityType == AbilityType.MiniAirStrike)
            {
                _castAbilityButton.SetAllowToLongClick(false);
            }
            else
            {
                _castAbilityButton.SetAllowToLongClick(false);
            }
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