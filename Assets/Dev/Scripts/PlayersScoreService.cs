using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev
{
    public class PlayersScoreService : NetworkContext
    {
        [SerializeField] private PlayersScoreUI _playersScoreUI;
        
        private List<PlayerScoreData> _playerScoreList = new List<PlayerScoreData>();
        private TeamsService _teamsService;
        private PlayersDataService _playersDataService;

        public override void Spawned()
        {
            if (HasStateAuthority == false) return;
            
            var playersSpawner = FindObjectOfType<PlayersSpawner>();
            playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            playersSpawner.DeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));

            _teamsService = FindObjectOfType<TeamsService>();

            _playersDataService = FindObjectOfType<PlayersDataService>();

            var playerHealthService = FindObjectOfType<PlayersHealthService>();
            playerHealthService.PlayerKilled.TakeUntilDestroy(this).Subscribe(UpdateTableScore);
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            PlayerScoreData playerScoreData = _playerScoreList.FirstOrDefault(x => x.PlayerId == playerRef);

            _playerScoreList.Remove(playerScoreData);
        }

        private void UpdateTableScore(PlayerDieEventContext context)
        {
            PlayerRef killerPlayer = context.Killer;
            PlayerRef deadPlayer = context.Killed;

            string killerPlayerName = _playersDataService.GetNickname(killerPlayer);
            string deadPlayerName = _playersDataService.GetNickname(deadPlayer);
            
            foreach (var player in _playerScoreList)
            {
                if (player.PlayerId == deadPlayer)
                {
                    player.PlayerDeathCount++;
                }

                if (player.PlayerId == killerPlayer)
                {
                    player.PlayerFragCount++;
                }
            }
            
            _playersScoreUI.UpdateScores(_playerScoreList.ToArray());
            
            Debug.Log($"{killerPlayerName} извиняется, за то что трахнул {deadPlayerName}");
        }


        private void OnPlayerSpawned(PlayerSpawnEventContext playerSpawnData)
        {
            var playerScoreData = new PlayerScoreData();

            PlayerRef playerId = playerSpawnData.PlayerRef;
            
            playerScoreData.PlayerId = playerId;
            playerScoreData.PlayerTeamSide = _teamsService.GetPlayerTeamSide(playerId);
            playerScoreData.Nickname = _playersDataService.GetNickname(playerId);
            playerScoreData.PlayerDeathCount = 0;
            playerScoreData.PlayerFragCount = 0;
            
            _playerScoreList.Add(playerScoreData);
            
            _playersScoreUI.UpdateScores(_playerScoreList.ToArray());
        }
    }
}