using System;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.UI.PopUpsAndMenus;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Levels.Interactions
{
    public class InteractionObjectsHandler : NetworkContext
    {
        private LevelService _levelService;
        private PopUpService _popUpService;
        private PlayersDataService _playersDataService;
        private HealthObjectsService _healthObjectsService;

        [Inject]
        private void Init(LevelService levelService, PopUpService popUpService, HealthObjectsService healthObjectsService, PlayersDataService playersDataService)
        {
            _healthObjectsService = healthObjectsService;
            _playersDataService = playersDataService;
            _popUpService = popUpService;
            _levelService = levelService;
        }

        protected override void CorrectState()
        {
            base.CorrectState();

            if (IsProxy)
            {
                OnLevelLoaded(_levelService.CurrentLevel);
            }
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _levelService.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
            _healthObjectsService.PlayerDied.TakeUntilDestroy(this).Subscribe((OnPlayerDied));
        }

        private void OnLevelLoaded(Level level)
        {
            foreach (InteractionObject interactionObject in _levelService.CurrentLevel.InteractionObjects)
            {
                interactionObject.PlayerTriggerZone.PlayerEntered.TakeUntilDestroy(this)
                    .Subscribe((player => OnPlayerZoneEntered(player, interactionObject)));

                interactionObject.PlayerTriggerZone.PlayerExit.TakeUntilDestroy(this)
                    .Subscribe((player => OnPlayerZoneExit(player, interactionObject)));
            }
        }

        private void OnPlayerDied(UnitDieContext context)
        {
            SessionPlayer sessionPlayer = context.Victim;
    
            if(Runner.LocalPlayer != sessionPlayer.Owner) return;
            
            OnInteractionObjectInteract(sessionPlayer.Owner,false, null);
        }

        private void OnPlayerZoneEntered(PlayerCharacter playerCharacter, InteractionObject interactionObject)
        {
            if(playerCharacter == null) return; // because of changing player character/spawning while standing in zone

            PlayerRef playerRef = playerCharacter.Object.InputAuthority;
            
            if(Runner.LocalPlayer != playerRef) return;

            OnInteractionObjectInteract(playerRef,true, interactionObject);
        }

        private void OnPlayerZoneExit(PlayerCharacter playerCharacter, InteractionObject interactionObject)
        {
            if(playerCharacter == null) return; // because of changing player character/spawning while standing in zone
            
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;

            if(Runner.LocalPlayer != playerRef) return;
            
            OnInteractionObjectInteract(playerRef,false, interactionObject);
        }

        private void OnInteractionObjectInteract([RpcTarget] PlayerRef target, bool isOn, InteractionObject interactionObject)
        {
            Action onInteraction;

            if (interactionObject == null)
            {
                isOn = false;
            }
            
            if (isOn)
            {
                if (interactionObject.IsAutoActive)
                {
                    onInteraction = null;
                    
                    interactionObject.AutoInteract(target);
                }
                else
                {
                    onInteraction = () =>
                    {
                        interactionObject.Interact(target);
                        Debug.Log($"Interaction with {interactionObject.name}!!", interactionObject);
                    };
                }
            }
            else
            {
                onInteraction = null;
            }
            
            _playersDataService.GetPlayerBase(target).PlayerController.SetInteractionAction(onInteraction);
        }
        
    }
}