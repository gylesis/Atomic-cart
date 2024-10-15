using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Fusion;
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

        private BotMovePoint _currentMovePoint;

        public PatrolBotState(Bot bot)
        {
            _bot = bot;
        }

        private Vector2 _botDirectionToMovePos;

        private TickTimer _searchForTargetsTimer;
        private TickTimer _changeMoveDirectionTimer;

        public void Enter()
        {
            SetRandomMovePos();

            _botDirectionToMovePos = _bot.DirectionToMovePos;

            ResetSearchForTargetsTimer();
            ResetSearchChangeDirectionTimer();
        }

        private void ResetSearchForTargetsTimer()
        {
            _searchForTargetsTimer = TickTimer.CreateFromSeconds(_bot.Runner, BotsConfig.BotsSearchForTargetsCooldown);
        }

        private void ResetSearchChangeDirectionTimer()
        {
            _changeMoveDirectionTimer =
                TickTimer.CreateFromSeconds(_bot.Runner, BotsConfig.BotsChangeMoveDirectionCooldown);
        }

        public void FixedNetworkTick()
        {
            if (_searchForTargetsTimer.ExpiredOrNotRunning(_bot.Runner))
            {
                ResetSearchForTargetsTimer();
                var foundTarget = _bot.TryFindNearTarget();

                if (foundTarget)
                    BotStateMachine.ChangeState<AttackPlayerBotState>();
            }

            if (_changeMoveDirectionTimer.ExpiredOrNotRunning(_bot.Runner))
            {
                ResetSearchChangeDirectionTimer();
                SetRandomMovePos();
            }

            if (_currentMovePoint == null)
                SetRandomMovePos();

            _bot.Move(_currentMovePoint.transform.position);
        }

        public void Tick() { }

        private void SetRandomMovePos()
        {
            Transform transform = _bot.transform;

            var movePoints = _bot.MovePoints.Where(x => !_usedPoints.Contains(x))
                .OrderBy(x => (x.transform.position - transform.position).sqrMagnitude).ToList();

            int allPointsCount = movePoints.Count;

            int maxPoints = BotsConfig.BotsNearestPointsAmountToChoose;
            int pointIndex = Random.Range(0, maxPoints);
            pointIndex = Math.Clamp(pointIndex, 0, allPointsCount);

            _currentMovePoint = movePoints[pointIndex];
            _bot.RandomMovePointPos = _currentMovePoint.transform.position;

            if (_usedPoints.Count > BotsConfig.PointsPoolAmount)
                _usedPoints.RemoveAt(0);

            _usedPoints.Add(_currentMovePoint);
        }

        public void Exit() { }
    }
}