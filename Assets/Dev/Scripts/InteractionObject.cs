using Dev.Infrastructure;
using Dev.Levels.Interactions;
using Fusion;
using UnityEngine;

namespace Dev
{
    public abstract class InteractionObject : NetworkContext
    {
        [SerializeField] private PlayerTriggerZone _playerTriggerZone;

        public PlayerTriggerZone PlayerTriggerZone => _playerTriggerZone;

        [Networked]
        public NetworkBool IsInteracted { get; private set; }

        protected override void CorrectState()
        {
            base.CorrectState();

            OnInteraction(IsInteracted);
        }

        [Rpc]
        public void RPC_Interact()
        {
            bool newValue = !IsInteracted;
            IsInteracted = newValue;

            if (Runner.IsSharedModeMasterClient)
            {
                OnInteraction(newValue);
            }
        }
      
        protected virtual void OnInteraction(bool interaction) { }
    }
}