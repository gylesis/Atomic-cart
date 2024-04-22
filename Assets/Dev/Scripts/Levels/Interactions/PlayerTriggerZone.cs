using Dev.PlayerLogic;
using UniRx;
using UnityEngine;

namespace Dev.Levels.Interactions
{
    public class PlayerTriggerZone : TriggerZone
    {
        [SerializeField] private TriggerZone _triggerZone;

        public Subject<PlayerCharacter> PlayerEntered { get; } = new Subject<PlayerCharacter>();
        public Subject<PlayerCharacter> PlayerExit { get; } = new Subject<PlayerCharacter>();

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _triggerZone.TriggerEntered.TakeUntilDestroy(this).Subscribe((OnZoneEntered));
            _triggerZone.TriggerExit.TakeUntilDestroy(this).Subscribe((OnZoneExit));
        }

        private void OnZoneEntered(Collider2D obj)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerCharacter playerCharacter = obj.GetComponent<PlayerCharacter>();

                PlayerEntered.OnNext(playerCharacter);
            }
        }

        private void OnZoneExit(Collider2D obj)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerCharacter playerCharacter = obj.GetComponent<PlayerCharacter>();

                PlayerExit.OnNext(playerCharacter);
            }
        }
    }
}