using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev
{
    public class TimeService : NetworkContext
    {
        [SerializeField] private Vector2Int _startTime;

        [SerializeField] private GameTimeView _gameTimeView;
        
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
            if(HasStateAuthority == false) return;

          //  _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
        }

        [ContextMenu(nameof(AddTime))]
        private void AddTime()
        {
            LeftTime = TickTimer.CreateFromSeconds( Runner,LeftTime.RemainingTime(Runner).Value + 200);
        }
        
        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            if (_playersSpawner.PlayersCount > 0)
            {
                if (LeftTime.ExpiredOrNotRunning(Runner))
                {
                    int minutesToSeconds = _startTime.x * 60;
                    int seconds = _startTime.y;

                    int overallSeconds = minutesToSeconds + seconds;

                    _lastIntTime = seconds;

                    TickTimer.CreateFromSeconds(Runner, overallSeconds);
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if(HasStateAuthority == false) return;

            if (LeftTime.ExpiredOrNotRunning(Runner) == false)
            {
                int remainingTime = (int) LeftTime.RemainingTime(Runner).Value;

                int seconds = remainingTime / 360;

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

    public struct TimeTickEventContext
    {
        public int LeftMinutes;
        public int LeftSeconds;
    }
    
}