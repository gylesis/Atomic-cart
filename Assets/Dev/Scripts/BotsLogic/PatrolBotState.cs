using System;
using Dev.Infrastructure;
using UniRx;
using UnityEngine;

namespace Dev.BotsLogic
{
    public class PatrolBotState : IBotState
    {
        private Bot _bot;
        private bool HasStateAuthority => _bot?.HasStateAuthority ?? false;
        private BotsConfig BotsConfig => _bot.BotStateController.GameSettings.BotsConfig;

        private StateMachine<IBotState> BotStateMachine => _bot.BotStateController.StateMachine;


        public PatrolBotState(Bot bot)
        {
            _bot = bot;
        }

        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private Vector2 _botDirectionToMovePos;

        public void Enter()
        {
            _botDirectionToMovePos = _bot.DirectionToMovePos;
            
            Observable
                .Interval(TimeSpan.FromSeconds(BotsConfig.BotsSearchForTargetsCooldown))
                .SkipWhile((l => HasStateAuthority == false))
                .Subscribe((l =>
                {
                    var foundTarget = _bot.TryFindNearTarget();

                    if (foundTarget)
                        BotStateMachine.ChangeState<AttackPlayerBotState>();
                })).AddTo(_compositeDisposable);

            Observable
                .Interval(TimeSpan.FromSeconds(BotsConfig.BotsChangeMoveDirectionCooldown))
                .SkipWhile((l => HasStateAuthority == false))
                .Subscribe((l => { _bot.ChangeMoveDirection(); })).AddTo(_compositeDisposable);
        }

        public void FixedNetworkTick()
        {
            _bot.Move(_bot.RandomMovePointPos);
        }

        public void Tick()
        {
            Vector2 direction = Vector2.Lerp(_botDirectionToMovePos, _bot.DirectionToMovePos, Time.deltaTime);
            _bot.AimWeaponTowards(direction);
            
            if(Time.frameCount % 10 == 0)
                _botDirectionToMovePos = _bot.DirectionToMovePos;
        }

        public void Exit()
        {
            _compositeDisposable?.Clear();
        }
    }
}