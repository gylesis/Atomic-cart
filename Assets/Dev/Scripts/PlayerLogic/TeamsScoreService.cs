using Dev.CartLogic;
using Dev.Infrastructure;
using Dev.Levels;
using Fusion;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class TeamsScoreService : NetworkContext
    {
        [SerializeField] private TMP_Text _blueScoreText;
        [SerializeField] private TMP_Text _redScoreText;

        private TeamsService _teamsService;
        private CartService _cartService;

        [Networked] private TeamScoreData BlueTeamScoreData { get; set; }
        [Networked] private TeamScoreData RedTeamScoreData { get; set; }


        [Inject]
        private void Init(TeamsService teamsService)
        {
            _teamsService = teamsService;
        }

        public override void Spawned()
        {
            LevelService.Instance.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
            
            if (HasStateAuthority == false)
            {
                RPC_UpdateTeamsScores();
                return;
            }

            BlueTeamScoreData = new TeamScoreData(TeamSide.Blue, 0);
            RedTeamScoreData = new TeamScoreData(TeamSide.Red, 0);

            RPC_UpdateTeamsScores();
        }

        private void OnLevelLoaded(Level level)
        {
            Debug.Log($"Level loaded");
            _cartService = level.CartService;
            level.CartService.PointReached.TakeUntilDestroy(this).Subscribe((unit => OnPointReached()));
        }

        private void OnPointReached()
        {
            if (Runner.IsSharedModeMasterClient == false) return;
            
            Debug.Log($"On point reached");
            TeamSide teamToCapturePoints = _cartService.TeamToCapturePoints;

            EvaluateTeamScore(teamToCapturePoints);
        }

        private void SetTeamScore(TeamSide teamSide, int score)
        {
            switch (teamSide)
            {
                case TeamSide.Blue:
                    BlueTeamScoreData = new TeamScoreData(teamSide, score);
                    break;
                case TeamSide.Red:
                    RedTeamScoreData = new TeamScoreData(teamSide, score);
                    break;
            }

            RPC_UpdateTeamsScores();
        }

        private void EvaluateTeamScore(TeamSide teamSide)
        {
            int score;

            switch (teamSide)
            {
                case TeamSide.Blue:
                    score = BlueTeamScoreData.Score + 1;
                    break;
                case TeamSide.Red:
                    score = RedTeamScoreData.Score + 1;
                    break;
                default:
                    score = 0;
                    break;
            }

            SetTeamScore(teamSide, score);
        }

        public void SwapTeamScores()
        {
            int blueTeamScore = BlueTeamScoreData.Score;
            int redTeamScore = RedTeamScoreData.Score;

            SetTeamScore(TeamSide.Blue, redTeamScore);
            SetTeamScore(TeamSide.Red, blueTeamScore);
        }

        public TeamScoreData GetWonTeam()
        {
            if (BlueTeamScoreData.Score > RedTeamScoreData.Score)
            {
                return BlueTeamScoreData;
            }
            else if (RedTeamScoreData.Score > BlueTeamScoreData.Score)
            {
                return RedTeamScoreData;
            }
            else
            {
                return RedTeamScoreData; // TODO what if draw
            }
        }

        public void ResetScores()
        {
            SetTeamScore(TeamSide.Blue, 0);
            SetTeamScore(TeamSide.Red, 0);
        }

        [Rpc]
        private void RPC_UpdateTeamsScores()
        {
            _blueScoreText.text = $"{BlueTeamScoreData.Score}";
            _redScoreText.text = $"{RedTeamScoreData.Score}";
        }
    }


    public struct TeamScoreData : INetworkStruct
    {
        public TeamSide Team { get; }
        [Networked] public int Score { get; private set; }

        public TeamScoreData(TeamSide team, int score)
        {
            Team = team;
            Score = score;
        }
    }
}