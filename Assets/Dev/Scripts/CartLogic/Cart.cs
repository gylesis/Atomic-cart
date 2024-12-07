using Cysharp.Threading.Tasks;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Levels.Interactions;
using Dev.PlayerLogic;
using Dev.Weapons.Guns;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Dev.CartLogic
{
    public class Cart : NetworkContext, IDamageable
    {
        public DamagableType DamageId => DamagableType.Obstacle;
        
        [FormerlySerializedAs("_interactionZone")] [SerializeField] private PlayerTriggerZone _triggerZone;
        private SessionStateService _sessionStateService;

        public Subject<SessionPlayer> CartZoneEntered { get; } = new Subject<SessionPlayer>();
        public Subject<SessionPlayer> CartZoneExit { get; } = new Subject<SessionPlayer>();

        [Inject]
        private void Construct(SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
        }
        
        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _triggerZone.PlayerEntered.Subscribe(OnPlayerEnteredCartZone).AddTo(GlobalDisposable.SceneScopeToken);
            _triggerZone.BotEntered.Subscribe(OnBotEnteredCartZone).AddTo(GlobalDisposable.SceneScopeToken);
            _triggerZone.PlayerExit.Subscribe(OnPlayerExitCartZone).AddTo(GlobalDisposable.SceneScopeToken);
            _triggerZone.BotExit.Subscribe(OnBotExitCartZone).AddTo(GlobalDisposable.SceneScopeToken);
        }

        private void OnBotEnteredCartZone(Bot bot)
        {
            var sessionPlayer = _sessionStateService.GetSessionPlayer(bot);
            
            if (!sessionPlayer.Equals(SessionPlayer.Default))
            {
                CartZoneEntered.OnNext(sessionPlayer);
            }
        }

        private void OnPlayerEnteredCartZone(PlayerCharacter playerCharacter)
        {
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;
            SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(playerRef);

            if (!sessionPlayer.Equals(SessionPlayer.Default))
            {
                CartZoneEntered.OnNext(sessionPlayer);
            }
        }

        private void OnBotExitCartZone(Bot bot)
        {
            SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(bot);

            if (!sessionPlayer.Equals(SessionPlayer.Default))
            {
                CartZoneExit.OnNext(sessionPlayer);
            }
        }

        private void OnPlayerExitCartZone(PlayerCharacter playerCharacter)
        {
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;
            SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(playerRef);

            if (!sessionPlayer.Equals(SessionPlayer.Default))
            {
                CartZoneExit.OnNext(sessionPlayer);
            }
        }
    }
}