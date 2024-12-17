using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev.BotsLogic
{
    public class AttackPlayerBotState : IBotState
    {
        private Bot _bot;
        private bool HasStateAuthority => _bot?.HasStateAuthority ?? false;
        private BotsConfig BotsConfig => _bot.BotStateController.GameSettings.BotsConfig;

        private StateMachine<IBotState> StateMachine => _bot.BotStateController.StateMachine;

        private TickTimer _tickTimer;

        public AttackPlayerBotState(Bot bot)
        {
            _bot = bot;
        }

        public void Enter()
        {
            _tickTimer = TickTimer.CreateFromSeconds(_bot.Runner, BotsConfig.SearchForTargetsCooldown);
        }

        public void Tick() { }

        public void FixedNetworkTick()
        {
            if (_tickTimer.ExpiredOrNotRunning(_bot.Runner))
            {
                _tickTimer = TickTimer.CreateFromSeconds(_bot.Runner, BotsConfig.SearchForTargetsCooldown);

                if (_bot.TryFindTarget(false) == false)
                    StateMachine.ChangeState<PatrolBotState>();
            }
           
            if (_bot.Target == null) return;

            float randomness = BotsConfig.ShootRandomnessFactor;
            Vector3 target = _bot.Target.transform.position + (Vector3)(Vector3.one * (Random.insideUnitCircle * randomness));
            
            Vector3 direction = (target - _bot.transform.position).normalized;
            
            _bot.AimWeaponTowards(direction);
            _bot.MoveTowardsTarget();

            if (_bot.AllowToShoot)
                _bot.WeaponController.TryToFire();
        }

        public void Exit() { }
    }
}