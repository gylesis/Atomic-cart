using Dev.Infrastructure;
using Dev.Levels.Interactions;
using Fusion;
using UnityEngine;

namespace Dev
{
    public abstract class InteractionObject : NetworkContext
    {
        [SerializeField] private bool _isAutoActive;
        [SerializeField] protected PlayerTriggerZone _playerTriggerZone;

        public PlayerTriggerZone PlayerTriggerZone => _playerTriggerZone;

        public bool IsAutoActive => _isAutoActive;

        [Networked]
        public NetworkBool IsInteracted { get; private set; }

        protected override void CorrectState()
        {
            base.CorrectState();

            OnInteraction(IsInteracted, PlayerRef.None); // bad. need another state recover
        }

        [Rpc]
        public void RPC_Interact(PlayerRef interactedPlayer)
        {
            bool newValue = !IsInteracted;
            IsInteracted = newValue;

            if (Runner.IsSharedModeMasterClient)
            {
                OnInteraction(newValue, interactedPlayer);    
            }
        }

        [Rpc]
        public void RPC_AutoInteract(PlayerRef interactedPlayer)
        {
            if (Runner.IsSharedModeMasterClient)
            {
                OnAutoInteraction(interactedPlayer);
            }   
        }
      
        protected virtual void OnAutoInteraction(PlayerRef interactedPlayer) { }
        protected virtual void OnInteraction(bool interaction, PlayerRef interactedPlayer) { }
    }
}   