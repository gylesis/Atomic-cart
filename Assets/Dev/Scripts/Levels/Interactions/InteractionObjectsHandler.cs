using System;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.UI;
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
        private PlayersHealthService _playersHealthService;

        [Inject]
        private void Init(LevelService levelService, PopUpService popUpService, PlayersHealthService playersHealthService)
        {
            _playersHealthService = playersHealthService;
            _popUpService = popUpService;
            _levelService = levelService;
        }

        protected override void OnSubscriptions()
        {
            _levelService.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
            _playersHealthService.PlayerKilled.TakeUntilDestroy(this).Subscribe((OnPlayerDied));

            base.OnSubscriptions();
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

        private void OnPlayerDied(PlayerDieEventContext context)
        {
            PlayerRef playerRef = context.Killed;

            RPC_SetInteractionViewState(playerRef,false, null);
        }

        private void OnPlayerZoneEntered(PlayerCharacter playerCharacter, InteractionObject interactionObject)
        {
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;

            RPC_SetInteractionViewState(playerRef,true, interactionObject);
        }

        private void OnPlayerZoneExit(PlayerCharacter playerCharacter, InteractionObject interactionObject)
        {
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;

            RPC_SetInteractionViewState(playerRef,false, interactionObject);
        }

        [Rpc]
        private void RPC_SetInteractionViewState([RpcTarget] PlayerRef target, bool isOn, InteractionObject interactionObject)
        {
            _popUpService.TryGetPopUp<HUDMenu>(out var hudMenu);

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
                    
                    interactionObject.RPC_AutoInteract(target);
                }
                else
                {
                    onInteraction = () =>
                    {
                        interactionObject.RPC_Interact(target);
                        Debug.Log($"Interaction with {interactionObject.name}!!", interactionObject);
                    };
                }
            }
            else
            {
                onInteraction = null;
            }
            
            hudMenu.SetInteractionAction(onInteraction);
        }
        
    }
}