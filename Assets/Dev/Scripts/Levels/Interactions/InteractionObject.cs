using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev.Levels.Interactions
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

        public void Interact(PlayerRef interactedPlayer)
        {
            bool newValue = !IsInteracted;
            IsInteracted = newValue;

            if (Runner.IsSharedModeMasterClient)
            {
                OnInteraction(newValue, interactedPlayer);    
            }
        }

        public void AutoInteract(PlayerRef interactedPlayer)
        {
            OnAutoInteraction(interactedPlayer);
        }
      
        protected virtual void OnAutoInteraction(PlayerRef interactedPlayer) { }
        protected virtual void OnInteraction(bool interaction, PlayerRef interactedPlayer) { }
    }
}   