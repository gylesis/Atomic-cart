using Dev.PlayerLogic;
using Dev.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Levels.Interactions
{
    public class PlayerInteractionZone : InteractionZone
    {
        [FormerlySerializedAs("_triggerZone")] [SerializeField] private InteractionZone _interactionZone;

        public Subject<Player> PlayerEntered { get; } = new Subject<Player>();
        public Subject<Player> PlayerExit { get; } = new Subject<Player>();

        protected override void ServerSubscriptions()
        {
            base.ServerSubscriptions();

            _interactionZone.TriggerEntered.TakeUntilDestroy(this).Subscribe((OnZoneEntered));
            _interactionZone.TriggerExit.TakeUntilDestroy(this).Subscribe((OnZoneExit));
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