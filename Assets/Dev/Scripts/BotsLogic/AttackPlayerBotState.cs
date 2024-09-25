using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
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

        private CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public AttackPlayerBotState(Bot bot)
        {
            _bot = bot;
        }

        public void Enter()
        {
            Observable
                .Interval(TimeSpan.FromSeconds(BotsConfig.BotsSearchForTargetsCooldown))
                .SkipWhile((l => HasStateAuthority == false))
                .Subscribe((l =>
                {
                    if (_bot.TryFindNearTarget() == false) 
                        StateMachine.ChangeState<PatrolBotState>();
                    
                })).AddTo(_compositeDisposable);
        }

        public void Tick() { }

        public void FixedNetworkTick()
        {
            if(_bot.Target == null) return;
            
            Vector3 direction = (_bot.Target.transform.position - _bot.transform.position).normalized;
            
            _bot.AimWeaponTowards(direction);
            _bot.MoveTowardsTarget();

            if (_bot.AllowToShoot) 
                _bot.WeaponController.TryToFire();
        }

        public void Exit()
        {
            _compositeDisposable?.Clear();
        }
    }
}