using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Fusion;
using UniRx;
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
            _tickTimer = TickTimer.CreateFromSeconds(_bot.Runner, BotsConfig.BotsSearchForTargetsCooldown);
        }

        public void Tick() { }

        public void FixedNetworkTick()
        {
            if (_tickTimer.ExpiredOrNotRunning(_bot.Runner))
            {
                _tickTimer = TickTimer.CreateFromSeconds(_bot.Runner, BotsConfig.BotsSearchForTargetsCooldown);

                if (_bot.TryFindNearTarget() == false)
                    StateMachine.ChangeState<PatrolBotState>();
            }
           
            if (_bot.Target == null) return;

            Vector3 direction = (_bot.Target.transform.position - _bot.transform.position).normalized;

            _bot.AimWeaponTowards(direction);
            _bot.MoveTowardsTarget();

            if (_bot.AllowToShoot)
                _bot.WeaponController.TryToFire();
        }

        public void Exit() { }
    }
}