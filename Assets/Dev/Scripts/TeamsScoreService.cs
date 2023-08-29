using System;
using Dev.Infrastructure;
using Fusion;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class TeamsScoreService : NetworkContext
    {
        [SerializeField] private TMP_Text _blueScoreText;
        [SerializeField] private TMP_Text _redScoreText;

        private TeamsService _teamsService;
        private CartPathService _cartPathService;

        [Networked] private TeamScoreData BlueTeamScoreData { get; set; }
        [Networked] private TeamScoreData RedTeamScoreData { get; set; }
    

        [Inject]
        private void Init(TeamsService teamsService, CartPathService cartPathService)
        {
            _teamsService = teamsService;
            _cartPathService = cartPathService;
        }
        
        public override void Spawned()
        {
            if (HasStateAuthority == false)
            {
                RPC_UpdateTeamsScores();
                return;
            }

            BlueTeamScoreData = new TeamScoreData(TeamSide.Blue, 0);
            RedTeamScoreData = new TeamScoreData(TeamSide.Red, 0);
            
            _cartPathService.PointReached.TakeUntilDestroy(this).Subscribe((unit => OnPointReached()));
            
            RPC_UpdateTeamsScores();
        }

        private void OnPointReached()
        {
            TeamSide teamToCapturePoints = _cartPathService.TeamToCapturePoints;

            switch (teamToCapturePoints)
            {
                case TeamSide.Blue:
                    BlueTeamScoreData = new TeamScoreData(teamToCapturePoints, BlueTeamScoreData.Score + 1);
                    break;
                case TeamSide.Red:
                    RedTeamScoreData = new TeamScoreData(teamToCapturePoints, RedTeamScoreData.Score + 1);
                    break;
            }

            RPC_UpdateTeamsScores();
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