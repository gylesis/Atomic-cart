using Cysharp.Threading.Tasks;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using UniRx;
using UnityEngine;

namespace Dev.Levels.Interactions
{
    public class PlayerTriggerZone : TriggerZone
    {
        [SerializeField] private TriggerZone _triggerZone;

        public Subject<PlayerCharacter> PlayerEntered { get; } = new Subject<PlayerCharacter>();
        public Subject<Bot> BotEntered { get; } = new Subject<Bot>();
        
        public Subject<PlayerCharacter> PlayerExit { get; } = new Subject<PlayerCharacter>();
        public Subject<Bot> BotExit { get; } = new Subject<Bot>();

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _triggerZone.TriggerEntered.Subscribe(OnZoneEntered).AddTo(GlobalDisposable.SceneScopeToken);
            _triggerZone.TriggerExit.Subscribe(OnZoneExit).AddTo(GlobalDisposable.SceneScopeToken);
        }

        private void OnZoneEntered(Collider2D collider)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerCharacter playerCharacter = collider.GetComponent<PlayerCharacter>();
                PlayerEntered.OnNext(playerCharacter);
            }
            else if (collider.CompareTag("Bot"))
            {
                Bot bot = collider.GetComponent<Bot>();
                BotEntered.OnNext(bot);
            }
        }

        private void OnZoneExit(Collider2D collider)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerCharacter playerCharacter = collider.GetComponent<PlayerCharacter>();
                PlayerExit.OnNext(playerCharacter);
            }
            else if (collider.CompareTag("Bot"))
            {
                Bot bot = collider.GetComponent<Bot>();
                BotExit.OnNext(bot);
            }
        }
    }
}