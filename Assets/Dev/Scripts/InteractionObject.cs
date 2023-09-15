using Dev.Infrastructure;
using Dev.Levels.Interactions;
using UnityEngine;

namespace Dev
{
    public abstract class InteractionObject : NetworkContext
    {
        [SerializeField] private PlayerTriggerZone _playerTriggerZone;

        public PlayerTriggerZone PlayerTriggerZone => _playerTriggerZone;
    }
}