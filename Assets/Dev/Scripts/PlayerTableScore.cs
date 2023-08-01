using System.Collections.Generic;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev
{
    public class PlayerTableScore : NetworkContext
    {
        public class PlayerScoreData
        {
            public PlayerRef PlayerName;
            public TeamSide PlayerTeamSide;
            public int PlayerFragCount;
            public int PlayerDeathCount;
        }

        


        private List<PlayerScoreData> _playerScoreList = new List<PlayerScoreData>();
        public override void Spawned()
        {
            if (HasStateAuthority == false) return;
            var playersSpawner = FindObjectOfType<PlayersSpawner>();
            playersSpawner.Spawned.Subscribe((OnPlayerSpawn));
            
            var playerHealthService = FindObjectOfType<PlayersHealthService>();
            playerHealthService.PlayerKilled.Subscribe(UpdateTableScore);
        }
        


        private void UpdateTableScore(PlayerDieEventContext playerDieEventContext)
        {
            foreach (var player in _playerScoreList)
            {
                if (player.PlayerName == playerDieEventContext.Killed)
                {
                    player.PlayerDeathCount++;
                }
                if (player.PlayerName == playerDieEventContext.Killer)
                {
                    player.PlayerFragCount++;
                }
            }
            Debug.Log($"{playerDieEventContext.Killer} извиняется, за то что трахнул {playerDieEventContext.Killed}");
        }


        private void OnPlayerSpawn(PlayerSpawnEventContext playerSpawnData)
        {
            var playerScoreData = new PlayerScoreData();
            var teamsService = FindObjectOfType<TeamsService>();
            playerScoreData.PlayerName = playerSpawnData.PlayerRef;
            playerScoreData.PlayerTeamSide = teamsService.GetPlayerTeamSide(playerSpawnData.PlayerRef);
            playerScoreData.PlayerDeathCount = 0;
            playerScoreData.PlayerFragCount = 0;
            _playerScoreList.Add(playerScoreData);
        }
        
        
    }
}