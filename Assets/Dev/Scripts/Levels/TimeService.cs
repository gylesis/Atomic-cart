﻿using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev
{
    public class TimeService : NetworkContext
    {
        [SerializeField] private GameTimeView _gameTimeView;
        [SerializeField] private TimeContainer _startTime;
        [SerializeField] private TimeContainer _timeRewardForCapturingPoint;

        private PlayersSpawner _playersSpawner;

        [Networked] private TickTimer LeftTime { get; set; }


        private int _lastIntTime = 0;
        private CartPathService _cartPathService;

        //public Subject<TimeTickEventContext> TimeTick { get; } = new Subject<TimeTickEventContext>();

        private void Awake()
        {
            _playersSpawner = FindObjectOfType<PlayersSpawner>();

            _cartPathService = FindObjectOfType<CartPathService>();
        }

        public override void Spawned()
        {
            if (HasStateAuthority == false) return;

            _cartPathService.PointReached.Subscribe(OnControlPoint);
            _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
        }

        private void OnControlPoint(Unit obj)
        {
            AddTime();
        }

        [ContextMenu(nameof(AddTime))]
        private void AddTime()
        {
            LeftTime = TickTimer.CreateFromSeconds(Runner, LeftTime.RemainingTime(Runner).Value + _timeRewardForCapturingPoint.OverallSeconds);
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            if (_playersSpawner.PlayersCount > 0)
            {
                if (LeftTime.ExpiredOrNotRunning(Runner))
                {
                    int overallSeconds = _startTime.OverallSeconds;

                    _lastIntTime = _startTime.Seconds;

                    LeftTime = TickTimer.CreateFromSeconds(Runner, overallSeconds);
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            if (LeftTime.ExpiredOrNotRunning(Runner) == false)
            {
                int remainingTime = (int)LeftTime.RemainingTime(Runner).Value;

                int seconds = remainingTime % 60;

                if (seconds != _lastIntTime)
                {
                    _lastIntTime = remainingTime;
                    TimeTickEventContext tickEventContext = new TimeTickEventContext();

                    tickEventContext.LeftMinutes = remainingTime / 60;
                    tickEventContext.LeftSeconds = seconds;

                    _gameTimeView.RPC_UpdateTime(tickEventContext);

                    //TimeTick.OnNext(tickEventContext);
                }
            }
        }
    }

    public struct TimeTickEventContext : INetworkStruct
    {
        public int LeftMinutes;
        public int LeftSeconds;
    }
}