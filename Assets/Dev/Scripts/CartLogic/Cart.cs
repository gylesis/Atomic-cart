using Dev.Infrastructure;
using Dev.Levels.Interactions;
using Dev.PlayerLogic;
using Dev.Weapons.Guns;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.CartLogic
{
    public class Cart : NetworkContext, IDamageable
    {
        public DamagableType DamageId => DamagableType.Obstacle;
        
        [FormerlySerializedAs("_interactionZone")] [SerializeField] private PlayerTriggerZone _triggerZone;

        public Subject<PlayerRef> CartZoneEntered { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> CartZoneExit { get; } = new Subject<PlayerRef>();

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _triggerZone.PlayerEntered.TakeUntilDestroy(this).Subscribe((OnPlayerEnteredCartZone));
            _triggerZone.PlayerExit.TakeUntilDestroy(this).Subscribe((OnPlayerExitCartZone));
        }

        private void OnPlayerEnteredCartZone(PlayerCharacter playerCharacter)
        {
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;

            CartZoneEntered.OnNext(playerRef);
        }

        private void OnPlayerExitCartZone(PlayerCharacter playerCharacter)
        {
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;

            CartZoneExit.OnNext(playerRef);
        }
    }
}