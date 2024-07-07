using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
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

        [Inject]
        private void Init(PlayersScoreService playersScoreService)
        {
            _playersScoreService = playersScoreService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();

            _playersScoreService.OnScoreUpdate.TakeUntilDestroy(this).Subscribe((unit => OnScoreUpdated()));
        }

        private void OnScoreUpdated()
        {
            UpdateScores(_playersScoreService.PlayerScoreList.ToArray());
        }

        private void UpdateScores(PlayerScoreData[] scoreDatas)
        {
            for (var index = 0; index < scoreDatas.Length; index++)
            {
                PlayerScoreData scoreData = scoreDatas[index];
                SessionPlayer sessionPlayer = scoreData.SessionPlayer;
                TeamSide teamSide = sessionPlayer.TeamSide;

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

            OrderScoreUI();
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
            var blueScoresUI = _scoreUis.Where(x => x.SessionPlayer.TeamSide == TeamSide.Blue).OrderByDescending(x => x.Kills).ThenBy(x => x.Deaths).ToList();
            var redScoresUI = _scoreUis.Where(x => x.SessionPlayer.TeamSide == TeamSide.Red).OrderByDescending(x => x.Kills).ThenBy(x => x.Deaths).ToList();
            
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
        
    }
}