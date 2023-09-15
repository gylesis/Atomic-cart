using Dev.Infrastructure;
using Dev.Levels.Interactions;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.CartLogic
{
    public class Cart : NetworkContext
    {
        [FormerlySerializedAs("_interactionZone")] [SerializeField] private PlayerTriggerZone _triggerZone;

        public Subject<PlayerRef> CartZoneEntered { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> CartZoneExit { get; } = new Subject<PlayerRef>();


        protected override void ServerSubscriptions()
        {
            base.ServerSubscriptions();

            _triggerZone.PlayerEntered.TakeUntilDestroy(this).Subscribe((OnPlayerEnteredCartZone));
            _triggerZone.PlayerExit.TakeUntilDestroy(this).Subscribe((OnPlayerExitCartZone));
        }

        private void OnPlayerEnteredCartZone(Player player)
        {
            PlayerRef playerRef = player.Object.InputAuthority;

            CartZoneEntered.OnNext(playerRef);
        }

        private void OnPlayerExitCartZone(Player player)
        {
            PlayerRef playerRef = player.Object.InputAuthority;

            CartZoneExit.OnNext(playerRef);
        }
    }
}