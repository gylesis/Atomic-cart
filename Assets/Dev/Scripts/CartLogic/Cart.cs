using System;
using Dev.Infrastructure;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev
{
    public class Cart : NetworkContext
    {
        [SerializeField] private PlayerTriggerBox _interactionZone;

        public Subject<PlayerRef> CartZoneEntered { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> CartZoneExit { get; } = new Subject<PlayerRef>();

        public override void Spawned()
        {   
            if(Runner.IsServer == false) return;

            _interactionZone.TriggerEntered.TakeUntilDestroy(this).Subscribe((OnPlayerEnteredCartZone));
            _interactionZone.TriggerExit.TakeUntilDestroy(this).Subscribe((OnPlayerExitCartZone));
        }

        private void OnPlayerExitCartZone(Collider2D other)
        {
            var tryGetComponent = other.TryGetComponent<Player>(out var player);

            if (tryGetComponent)
            {
                PlayerRef playerRef = player.Object.InputAuthority;
                
                CartZoneExit.OnNext(playerRef);
            }
        }

        private void OnPlayerEnteredCartZone(Collider2D other)
        {
            var tryGetComponent = other.TryGetComponent<Player>(out var player);

            if (tryGetComponent)
            {
                PlayerRef playerRef = player.Object.InputAuthority;
                
                CartZoneEntered.OnNext(playerRef);
            }
            
        }
    }
}