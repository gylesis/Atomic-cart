using Dev.Infrastructure;
using Dev.Levels.Interactions;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.CartLogic
{
    public class Cart : NetworkContext
    {
        [SerializeField] private PlayerInteractionZone _interactionZone;

        public Subject<PlayerRef> CartZoneEntered { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> CartZoneExit { get; } = new Subject<PlayerRef>();


        protected override void ServerSubscriptions()
        {
            base.ServerSubscriptions();

            _interactionZone.PlayerEntered.TakeUntilDestroy(this).Subscribe((OnPlayerEnteredCartZone));
            _interactionZone.PlayerExit.TakeUntilDestroy(this).Subscribe((OnPlayerExitCartZone));
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