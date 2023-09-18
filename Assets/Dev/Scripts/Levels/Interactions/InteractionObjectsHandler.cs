using System;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev
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

        protected override void ServerSubscriptions()
        {
            base.ServerSubscriptions();

            _playersHealthService.PlayerKilled.TakeUntilDestroy(this).Subscribe((OnPlayerDied));
            
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

        private void OnPlayerZoneEntered(Player player, InteractionObject interactionObject)
        {
            PlayerRef playerRef = player.Object.InputAuthority;

            RPC_SetInteractionViewState(playerRef,true, interactionObject);
        }

        private void OnPlayerZoneExit(Player player, InteractionObject interactionObject)
        {
            PlayerRef playerRef = player.Object.InputAuthority;

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