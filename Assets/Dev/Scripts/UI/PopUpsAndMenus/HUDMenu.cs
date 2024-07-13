using System;
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

        [SerializeField] private DefaultReactiveButton _toggleCastModesButton;

        [SerializeField] private LongClickReactiveButton _resetAbilityButton;
        
        private PlayerBase _playerBase;
        private AbilityCastController _castController;
        private PlayersSpawner _playersSpawner;
        private JoysticksContainer _joysticksContainer;

        public Subject<Unit> CastButtonClicked { get; } = new Subject<Unit>();
        public Subject<Unit> InteractiveButtonClicked { get; } = new Subject<Unit>();

        private bool IsCastingMode => _playerBase.PlayerController.IsCastingMode;

        [Inject]
        private void Construct(PlayersSpawner playersSpawner, JoysticksContainer joysticksContainer)
        {
            _joysticksContainer = joysticksContainer;
            _playersSpawner = playersSpawner;
        }

        private void Start()
        {
            _showTab.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnShowTabButtonClicked()));
            _exitMenuButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitMenuButtonClicked()));
            _interactionButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnInteractionButtonClicked()));
            
            _resetAbilityButton.LongClick.TakeUntilDestroy(this).Subscribe((unit => OnResetAbilityButtonClicked()));
            _resetAbilityButton.SetAllowToLongClick(true);

            _toggleCastModesButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnToggleCastModesClicked()));
            
            _joysticksContainer.AimJoystick.PointerUpOrDown.TakeUntilDestroy(this)
                .Subscribe((OnAimJoystickPointerUpOrDown));

            _playersSpawner.PlayerBaseSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerBaseSpawned));
            _playersSpawner.PlayerCharacterSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerCharacterSpawned));
        }

        private void OnPlayerBaseSpawned(PlayerSpawnEventContext context)
        {
            if (_playerBase != null) return;

            NetworkRunner runner = FindObjectOfType<NetworkRunner>();

            PlayerRef playerRef = context.PlayerRef;

            if (runner.LocalPlayer != playerRef) return;

            PlayerBase playerBase = _playersSpawner.GetPlayerBase(playerRef);

            _playerBase = playerBase;

            _castController = playerBase.AbilityCastController;
            _castController.AbilityRecharged.TakeUntilDestroy(this).Subscribe((OnAbilityRecharged));
            
            SetCastMode(IsCastingMode);
        }

        private void OnPlayerCharacterSpawned(PlayerSpawnEventContext context)
        {
            SetCastMode(IsCastingMode);
            SetResetAbilityVisible(_castController.CurrentAbilityToCast, false);
        }

        private void CastAbility()
        {
            if (_castController.AllowToCast == false) return;

            CastButtonClicked.OnNext(Unit.Default);

            SetResetAbilityVisible(_castController.CurrentAbilityToCast, true);
            
            PlayerInput input = new PlayerInput();
            input.WithCast(true);
            
            _playerBase.InputService.SimulateInput(input);
            
            return;
            OnToggleCastModesClicked();
        }

        private void SetResetAbilityVisible(AbilityType abilityType, bool toCast)
        {
            switch (abilityType)
            {
                case AbilityType.Landmine:
                case AbilityType.Turret:
                case AbilityType.TearGas:
                    if (toCast)
                    {
                        _resetAbilityButton.Enable();
                    }
                    else
                    {
                        _resetAbilityButton.Disable();
                    }
                    break;
                case AbilityType.MiniAirStrike:
                    _resetAbilityButton.Disable();
                    break;
            }
        }

        private void OnResetAbilityButtonClicked()
        {
            Debug.Log($"Reset button clicked");
            //_resetAbilityButton.SetAllowToLongClick(false);

            if (_castController.CurrentAbilityToCast == AbilityType.MiniAirStrike)
            {
                return;
            }

            _resetAbilityButton.ResetProgressImage();
            _castController.ResetAbility();
        }

        private void OnAbilityRecharged(AbilityType abilityType)
        {
            SetResetAbilityVisible(abilityType, false);
            
            Debug.Log($"Ability recharged");
        }
        
        private void OnToggleCastModesClicked()
        {
            _playerBase.PlayerController.IsCastingMode = !IsCastingMode;
            SetCastMode(IsCastingMode);
        }

        private void SetCastMode(bool isCasting)
        {
            _resetAbilityButton.gameObject.SetActive(isCasting);
            
            _joysticksContainer.AimJoystick.SetThresholdImageState(!isCasting);
        }
        
        private void OnAimJoystickPointerUpOrDown(bool isUp)
        {
            if(isUp == false) return;
            
            if(_playerBase.PlayerController.IsCastingMode == false) return;
            
            CastAbility();
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