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
        private PlayersDataService _playersDataService;

        [Inject]
        private void Init(LevelService levelService, PopUpService popUpService, PlayersHealthService playersHealthService, PlayersDataService playersDataService)
        {
            _playersDataService = playersDataService;
            _playersHealthService = playersHealthService;
            _popUpService = popUpService;
            _levelService = levelService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _levelService.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
            _playersHealthService.PlayerKilled.TakeUntilDestroy(this).Subscribe((OnPlayerDied));
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

            RPC_OnInteractionObjectInteract(playerRef,false, null);
        }

        private void OnPlayerZoneEntered(PlayerCharacter playerCharacter, InteractionObject interactionObject)
        {
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;

            RPC_OnInteractionObjectInteract(playerRef,true, interactionObject);
        }

        private void OnPlayerZoneExit(PlayerCharacter playerCharacter, InteractionObject interactionObject)
        {
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;

            RPC_OnInteractionObjectInteract(playerRef,false, interactionObject);
        }

        [Rpc]
        private void RPC_OnInteractionObjectInteract([RpcTarget] PlayerRef target, bool isOn, InteractionObject interactionObject)
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
            
            _playersDataService.GetPlayer(target).PlayerController.SetInteractionAction(onInteraction);
        }
        
    }
}