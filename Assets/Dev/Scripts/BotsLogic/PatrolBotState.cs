using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.BotsLogic
{
    public class PatrolBotState : IBotState
    {
        private Bot _bot;
        private bool HasStateAuthority => _bot?.HasStateAuthority ?? false;
        private BotsConfig BotsConfig => _bot.BotStateController.GameSettings.BotsConfig;

        private StateMachine<IBotState> BotStateMachine => _bot.BotStateController.StateMachine;

        List<BotMovePoint> _usedPoints = new List<BotMovePoint>();

        private int _currentPointIndex = 0;
        private BotMovePoint _currentMovePoint;
        private List<BotMovePoint> _movePoints => _bot.MovePoints;
            
        public PatrolBotState(Bot bot)
        {
            _bot = bot;
        }

        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private Vector2 _botDirectionToMovePos;

        public void Enter()
        {
            SetRandomMovePos();
            
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
                .Subscribe((l =>
                {
                    SetRandomMovePos();
                })).AddTo(_compositeDisposable);
        }

        public void FixedNetworkTick()
        {
            if(_currentMovePoint == null)
                SetRandomMovePos();
            
            _bot.Move(_currentMovePoint.transform.position);
        }

        public void Tick()
        {
            Vector2 direction = Vector2.Lerp(_botDirectionToMovePos, _bot.DirectionToMovePos, Time.deltaTime);
            _bot.AimWeaponTowards(direction);
            
            if(Time.frameCount % 10 == 0)
                _botDirectionToMovePos = _bot.DirectionToMovePos;
        }

        private void SetRandomMovePos()
        {
            Transform transform = _bot.transform;

            var movePoints = _bot.MovePoints.Where(x => !_usedPoints.Contains(x))
                .OrderBy(x => (x.transform.position - transform.position).sqrMagnitude).ToList();

            int allPointsCount = movePoints.Count;

            int maxPoints = BotsConfig.BotsNearestPointsAmountToChoose;
            int pointIndex = Random.Range(0, maxPoints);
            pointIndex = Math.Clamp(pointIndex, 0, allPointsCount);

            _currentPointIndex = pointIndex;
            _currentMovePoint = movePoints[pointIndex];
            _bot.RandomMovePointPos = _currentMovePoint.transform.position;

            if (_usedPoints.Count > BotsConfig.PointsPoolAmount) 
                _usedPoints.RemoveAt(0);

            _usedPoints.Add(_currentMovePoint);
        }

        public void Exit()
        {
            _compositeDisposable?.Clear();
        }
    }
}