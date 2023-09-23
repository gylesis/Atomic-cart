using System;
using Dev.CartLogic;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class GameService : NetworkContext
    {
        private TimeService _timeService;
        private PlayersSpawner _playersSpawner;
        private PopUpService _popUpService;
        private GameSettings _gameSettings;
        private TeamsScoreService _teamsScoreService;
        private TeamsService _teamsService;

        private bool _teamsSwapHappened;
        
        private CartPathService _cartPathService;

        public Subject<Unit> GameRestarted { get; } = new Subject<Unit>();

        [Inject]
        private void Init(TimeService timeService, PlayersSpawner playersSpawner,
            PopUpService popUpService, GameSettings gameSettings, TeamsScoreService teamsScoreService,
            TeamsService teamsService)
        {
            _teamsService = teamsService;
            _teamsScoreService = teamsScoreService;
            _gameSettings = gameSettings;
            _popUpService = popUpService;
            _timeService = timeService;
            _playersSpawner = playersSpawner;
        }

        protected override void ServerSubscriptions()
        {
            LevelService.Instance.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));

            base.ServerSubscriptions();
            
            _timeService.GameTimeRanOut.TakeUntilDestroy(this).Subscribe((unit => OnGameTimeRanOut()));
        }

        private void OnLevelLoaded(Level level)
        {
            level.CartPathService.PointReached.TakeUntilDestroy(this).Subscribe((unit => OnPointReached()));
            _cartPathService = level.CartPathService;
        }

        private void OnPointReached()
        {
            if (_cartPathService.IsOnLastPoint == false) return;

            _timeService.SetPauseState(true);

            if (_teamsSwapHappened == false)
            {
                RestartGame(_gameSettings.TimeAfterWinGame, (() =>
                {
                    _teamsSwapHappened = true;

                    _teamsScoreService.SwapTeamScores();
                    _teamsService.SwapTeams();
                }));

                string title = $"Restarting game";
                TeamSide teamToCapturePoints = _cartPathService.TeamToCapturePoints;
                string colorTag = teamToCapturePoints == TeamSide.Red ? "red" : "blue";
                string description =
                    $"Team <color={colorTag}>{teamToCapturePoints}</color> captured all control points";
                int timeAfterWinGame = _gameSettings.TimeAfterWinGame;

                RPC_ShowRestartNotification(title, description, timeAfterWinGame);
            }
            else
            {
                RestartGame(_gameSettings.TimeAfterWinGame, (() =>
                {
                    _teamsSwapHappened = false;
                    _teamsScoreService.ResetScores();
                }));


                TeamScoreData wonTeamScoreData = _teamsScoreService.GetWonTeam();
                TeamSide wonTeam = wonTeamScoreData.Team;

                string title = $"End of the game";
                string colorTag = wonTeam == TeamSide.Red ? "red" : "blue";
                string description = $"Team <color={colorTag}>{wonTeam}</color> most scored!";
                int timeAfterWinGame = _gameSettings.TimeAfterWinGame;

                RPC_ShowRestartNotification(title, description, timeAfterWinGame);
            }
        }


        [ContextMenu(nameof(SimulateReachLastPoint))]
        private void SimulateReachLastPoint()
        {
            _timeService.SetPauseState(true);

            RestartGame(_gameSettings.TimeAfterWinGame);

            string title = $"Restarting game";
            TeamSide teamToCapturePoints = _cartPathService.TeamToCapturePoints;
            string colorTag = teamToCapturePoints == TeamSide.Red ? "red" : "blue";
            string description = $"Team <color={colorTag}>{teamToCapturePoints}</color> captured all control points";
            int timeAfterWinGame = _gameSettings.TimeAfterWinGame;

            RPC_ShowRestartNotification(title, description, timeAfterWinGame);
        }

        [Rpc]
        private void RPC_ShowRestartNotification(string title, string description, int removeNotificationDelay)
        {
            _popUpService.TryGetPopUp<NotificationPopUp>(out var notificationPopUp);

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
            Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe((l =>
            {
                onRestarted?.Invoke();
                _cartPathService.ResetCart();
                RespawnAllPlayers();
                SetEnemiesFreezeState(false);
                _timeService.ResetTimer();
                _timeService.SetPauseState(false);
                
                GameRestarted.OnNext(Unit.Default);
            }));
        }

        private void SetEnemiesFreezeState(bool toFreeze)
        {
            foreach (Player player in _playersSpawner.Players)
            {
                player.PlayerController.AllowToMove = !toFreeze;
                player.PlayerController.AllowToShoot = !toFreeze;
            }
        }

        private void RespawnAllPlayers()
        {
            foreach (Player player in _playersSpawner.Players)
            {
                _playersSpawner.RespawnPlayer(player.Object.InputAuthority);
            }
        }
    }
}