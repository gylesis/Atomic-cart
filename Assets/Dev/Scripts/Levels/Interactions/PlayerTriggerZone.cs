using Dev.PlayerLogic;
using Dev.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Levels.Interactions
{
    public class PlayerTriggerZone : TriggerZone
    {
        [SerializeField] private TriggerZone _triggerZone;

        public Subject<Player> PlayerEntered { get; } = new Subject<Player>();
        public Subject<Player> PlayerExit { get; } = new Subject<Player>();

        protected override void ServerSubscriptions()
        {
            base.ServerSubscriptions();

            _triggerZone.TriggerEntered.TakeUntilDestroy(this).Subscribe((OnZoneEntered));
            _triggerZone.TriggerExit.TakeUntilDestroy(this).Subscribe((OnZoneExit));
        }

        private void OnZoneEntered(Collider2D obj)
        {
            if (obj.CompareTag("Player"))
            {
                Player player = obj.GetComponent<Player>();

                PlayerEntered.OnNext(player);
            }
        }

        private void OnZoneExit(Collider2D obj)
        {
            if (obj.CompareTag("Player"))
            {
                Player player = obj.GetComponent<Player>();

                PlayerExit.OnNext(player);
            }
        }
    }
}