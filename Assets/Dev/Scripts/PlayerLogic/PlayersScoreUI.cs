using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Utils;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersScoreUI : NetworkContext
    {
        [SerializeField] private PlayerScoreUIContainer _blueTeamScores;
        [SerializeField] private PlayerScoreUIContainer _redTeamScores;

        [SerializeField] private PlayerScoreUI _playerScoreUIPrefab;

        [SerializeField] private Color _localPlayerColor;

        private PlayersScoreService _playersScoreService;

        private List<PlayerScoreUI> _scoreUis = new List<PlayerScoreUI>();
        private SessionStateService _sessionStateService;

        [Inject]
        private void Init(PlayersScoreService playersScoreService, SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
            _playersScoreService = playersScoreService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();

            _playersScoreService.OnScoreUpdate.TakeUntilDestroy(this).Subscribe((unit => OnScoreUpdated()));
        }

        private void OnScoreUpdated()
        {
            UpdateScores(_playersScoreService.PlayerScoreList);
        }

        private void UpdateScores(IEnumerable<PlayerScoreData> scoreDatas)
        {
            foreach (var scoreData in scoreDatas)
            {
                SessionPlayer sessionPlayer = scoreData.SessionPlayer;
                Result hasTeamResult = _sessionStateService.TryGetPlayerTeam(sessionPlayer, out TeamSide teamSide);

                if (hasTeamResult == false)
                {
                    AtomicLogger.Err(hasTeamResult.ErrorMessage);
                    continue;
                }

                PlayerScoreUI scoreUI = _scoreUis.FirstOrDefault(x => x.SessionPlayer.Id == sessionPlayer.Id);

                if (scoreUI != null)
                {
                    scoreUI.UpdateData(scoreData.PlayerFragCount, scoreData.PlayerDeathCount);
                    // scoreUI.InitNickname(scoreData.SessionPlayer.Name);  // TODO make on change nickname event
                }
                else
                {
                    scoreUI = Instantiate(_playerScoreUIPrefab, GetTeamUIParent(teamSide));

                    scoreUI.Init(sessionPlayer, scoreData.PlayerFragCount, scoreData.PlayerDeathCount);

                    _scoreUis.Add(scoreUI);
                }

                scoreUI.SetHighlightColor(Color.clear);

                if (sessionPlayer.IsBot == false)
                {
                    if (sessionPlayer.Owner == Runner.LocalPlayer)
                    {
                        scoreUI.SetHighlightColor(_localPlayerColor);
                    }
                }
            }

            RemoveLeftPlayers();

            OrderScoreUI();
        }

        private void RemoveLeftPlayers()
        {
            var playerScoreUis = _scoreUis
                .Where(x => _sessionStateService.DoPlayerExist(x.SessionPlayer.Owner) == false).ToList();

            foreach (PlayerScoreUI scoreUi in playerScoreUis)
            {
                _scoreUis.Remove(scoreUi);
                Destroy(scoreUi.gameObject);
            }
        }

        private Transform GetTeamUIParent(TeamSide teamSide)
        {
            if (teamSide == TeamSide.Blue)
            {
                return _blueTeamScores.ScoresUIParent;
            }
            else
            {
                return _redTeamScores.ScoresUIParent;
            }
        }

        private void OrderScoreUI()
        {
            var blueScoresUI = GetScoreUis(TeamSide.Blue);
            var redScoresUI = GetScoreUis(TeamSide.Red);

            for (var index = 0; index < blueScoresUI.Count; index++)
            {
                PlayerScoreUI scoreUI = blueScoresUI[index];

                scoreUI.transform.SetSiblingIndex(index);
            }

            for (var index = 0; index < redScoresUI.Count; index++)
            {
                PlayerScoreUI scoreUI = redScoresUI[index];

                scoreUI.transform.SetSiblingIndex(index);
            }
        }


        private List<PlayerScoreUI> GetScoreUis(TeamSide targetTeam)
        {
            return _scoreUis
                .Where(x =>
                {
                    var hasTeam = _sessionStateService.TryGetPlayerTeam(x.SessionPlayer, out TeamSide teamSide);
                    return hasTeam && teamSide == targetTeam;
                })
                .OrderByDescending(x => x.Kills)
                .ThenBy(x => x.Deaths)
                .ToList();
        }
    }
}