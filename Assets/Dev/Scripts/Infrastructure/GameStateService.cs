using System;
using Dev.CartLogic;
using Dev.Infrastructure.Networking;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class GameStateService : NetworkContext
    {
        private TimeService _timeService;
        private PopUpService _popUpService;
        private GameSettings _gameSettings;
        private TeamsScoreService _teamsScoreService;
        private TeamsService _teamsService;

        private bool _teamsSwapHappened;

        private CartService _cartService;
        private LevelService _levelService;
        private SessionStateService _sessionStateService;

        public Subject<Unit> GameRestarted { get; } = new Subject<Unit>();

        [Inject]
        private void Init(TimeService timeService, SessionStateService sessionStateService,
            PopUpService popUpService, GameSettings gameSettings, TeamsScoreService teamsScoreService,
            TeamsService teamsService, LevelService levelService)
        {
            _sessionStateService = sessionStateService;
            _levelService = levelService;
            _teamsService = teamsService;
            _teamsScoreService = teamsScoreService;
            _gameSettings = gameSettings;
            _popUpService = popUpService;
            _timeService = timeService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _levelService.LevelLoaded.Subscribe(OnLevelLoaded).AddTo(this);
            _timeService.GameTimeRanOut.Subscribe(unit => OnGameTimeRanOut()).AddTo(this);
        }

        private void OnLevelLoaded(Level level)
        {
            level.CartService.PointReached.Subscribe(unit => OnPointReached()).AddTo(level);
            _cartService = level.CartService;
        }

        private void OnPointReached()
        {
            if (Runner.IsSharedModeMasterClient == false) return;
            if (_cartService.IsOnLastPoint == false) return;

            _timeService.SetPauseState(true);

            TeamSide teamToCapturePoints = _cartService.DragTeamSide;
            string colorTag = teamToCapturePoints == TeamSide.Red ? "red" : "blue";
            
            string title = _teamsSwapHappened ? "End of the game" : "Restarting game";
            string description = _teamsSwapHappened ? $"Team <color={colorTag}>{_teamsScoreService.GetWonTeam()}</color> most scored!" : $"Team <color={colorTag}>{teamToCapturePoints}</color> captured all control points";
            
            int timeAfterWinGame = _gameSettings.TimeAfterWinGame;

            Action onRestarted;
            
            if (_teamsSwapHappened)
            {
                onRestarted = () =>
                {
                    _teamsSwapHappened = false;
                    _teamsScoreService.ResetScores();
                };
            }
            else
            {
                onRestarted = () =>
                {
                    _teamsSwapHappened = true;

                    _teamsScoreService.SwapTeamScores();
                    _teamsService.SwapTeams();
                };
            }

            RestartGame(timeAfterWinGame, onRestarted);
            RPC_ShowRestartNotification(title, description, timeAfterWinGame);
        }


        [ContextMenu(nameof(SimulateReachLastPoint))]
        private void SimulateReachLastPoint()
        {
            _timeService.SetPauseState(true);
            
            Action onRestarted;
            
            if (_teamsSwapHappened)
            {
                onRestarted = () =>
                {
                    _teamsSwapHappened = false;
                    _teamsScoreService.ResetScores();
                };
            }
            else
            {
                onRestarted = () =>
                {
                    _teamsSwapHappened = true;

                    _teamsScoreService.SwapTeamScores();
                    _teamsService.SwapTeams();
                };
            }
            
            RestartGame(_gameSettings.TimeAfterWinGame, onRestarted);

            string title = $"Restarting game";
            TeamSide teamToCapturePoints = _cartService.DragTeamSide;
            string colorTag = teamToCapturePoints == TeamSide.Red ? "red" : "blue";
            string description = $"Team <color={colorTag}>{teamToCapturePoints}</color> captured all control points";
            int timeAfterWinGame = _gameSettings.TimeAfterWinGame;

            RPC_ShowRestartNotification(title, description, timeAfterWinGame);
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        private void RPC_ShowRestartNotification(string title, string description, int removeNotificationDelay)
        {
            var notificationPopUp = _popUpService.ShowPopUp<NotificationPopUp>();

            notificationPopUp.Init(title, description, removeNotificationDelay);
            notificationPopUp.Show();
        }

        [ContextMenu("Restart")]
        private void OnGameTimeRanOut()
        {
            string title = $"Restarting game";
            string description = $"No team reached last point";
            int delay = 5;

            RPC_ShowRestartNotification(title, description, delay);
            RestartGame(delay + 1);
        }

        private void RestartGame(float delay = 0, Action onRestarted = null)
        {
            Extensions.Delay(delay, destroyCancellationToken, (() =>
            {
                onRestarted?.Invoke();
                _cartService.ResetCart();
                _sessionStateService.RespawnAllPlayers();
                _sessionStateService.SetEnemiesFreezeState(false);
                _timeService.ResetTimer();
                _timeService.SetPauseState(false);
                
                GameRestarted.OnNext(Unit.Default);
            }));
          
        }
       
    }
}