using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersScoreService : NetworkContext
    {
        [SerializeField] private PlayersScoreUI _playersScoreUI;

        private List<PlayerScoreData> _playerScoreList = new List<PlayerScoreData>();
        private TeamsService _teamsService;
        private PlayersDataService _playersDataService;
        private PlayersSpawner _playersSpawner;
        private PlayersHealthService _playersHealthService;

        [Inject]
        private void Init(TeamsService teamsService, PlayersDataService playersDataService,
            PlayersSpawner playersSpawner, PlayersHealthService playersHealthService)
        {
            _teamsService = teamsService;
            _playersDataService = playersDataService;
            _playersHealthService = playersHealthService;
            _playersSpawner = playersSpawner;
        }

        public override void Spawned()
        {
            _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            _playersSpawner.DeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));

            _playersHealthService.PlayerKilled.TakeUntilDestroy(this).Subscribe(UpdateTableScore);
        }

        private async void OnPlayerSpawned(PlayerSpawnEventContext playerSpawnData)
        {
            await Task.Delay(500);
            
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

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            PlayerScoreData playerScoreData = _playerScoreList.FirstOrDefault(x => x.PlayerId == playerRef);

            _playerScoreList.Remove(playerScoreData);
        }

        private void UpdateTableScore(PlayerDieEventContext context)
        {
            PlayerRef killerPlayer = context.Killer;
            PlayerRef deadPlayer = context.Killed;

            bool isKilledByServer = killerPlayer == PlayerRef.None;
            
            string killerName;

            if (isKilledByServer)
            {
                killerName = "Server";
            }
            else
            {
                killerName = _playersDataService.GetNickname(killerPlayer);
            }
            
            string deadName = _playersDataService.GetNickname(deadPlayer);

            foreach (var player in _playerScoreList)
            {
                if (player.PlayerId == deadPlayer)
                {
                    player.PlayerDeathCount++;
                }

                if (isKilledByServer == false)
                {
                    if (player.PlayerId == killerPlayer)
                    {
                        player.PlayerFragCount++;
                    }
                }
            }

            _playersScoreUI.UpdateScores(_playerScoreList.ToArray());

            Debug.Log($"{killerName} killed {deadName}");
        }
    }

}