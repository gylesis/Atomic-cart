using Dev.CartLogic;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Levels
{
    public class TimeService : NetworkContext
    {
        [SerializeField] private GameTimeView _gameTimeView;
        [SerializeField] private TimeContainer _startTime;
        [SerializeField] private TimeContainer _timeRewardForCapturingPoint;

        private PlayersSpawner _playersSpawner;

        [Networked] private TickTimer LeftTime { get; set; }


        private int _lastIntTime = 0;

        //public Subject<TimeTickEventContext> TimeTick { get; } = new Subject<TimeTickEventContext>();

        public Subject<Unit> GameTimeRanOut { get; } = new Subject<Unit>();

        [Networked] public NetworkBool IsPaused { get; private set; }

        [Inject]
        private void Init(PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
        }

        public override void Spawned()
        {
            base.Spawned();
            
            LevelService.Instance.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
            _playersSpawner.BaseSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
        }

        private void OnLevelLoaded(Level level)
        {
            level.CartService.PointReached.TakeUntilDestroy(this).Subscribe(OnControlPoint);
        }

        public void SetPauseState(bool isPause)
        {
            IsPaused = isPause;
        }

        public void ResetTimer()
        {
            int overallSeconds = _startTime.OverallSeconds;

            _lastIntTime = _startTime.Seconds;

            LeftTime = TickTimer.CreateFromSeconds(Runner, overallSeconds);
        }

        private void OnControlPoint(Unit obj)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            AddTime();
        }

        [ContextMenu(nameof(AddTime))]
        private void AddTime()
        {
            LeftTime = TickTimer.CreateFromSeconds(Runner,
                LeftTime.RemainingTime(Runner).Value + _timeRewardForCapturingPoint.OverallSeconds);
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            if (_playersSpawner.PlayersCount > 0)
            {
                if (LeftTime.ExpiredOrNotRunning(Runner))
                {
                    ResetTimer();
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            if (IsPaused) return;

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

                if (remainingTime == 0)
                {
                    GameTimeRanOut.OnNext(Unit.Default);
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