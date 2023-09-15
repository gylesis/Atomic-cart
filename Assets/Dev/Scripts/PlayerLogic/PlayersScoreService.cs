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

        [Networked, Capacity(10)] private NetworkLinkedList<PlayerScoreData> PlayerScoreList { get; }
       
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
            
            if(Runner.IsSharedModeMasterClient == false) return;
            
            var playerScoreData = new PlayerScoreData();

            PlayerRef playerId = playerSpawnData.PlayerRef;

            playerScoreData.PlayerId = playerId;
            playerScoreData.PlayerTeamSide = _teamsService.GetPlayerTeamSide(playerId);
            playerScoreData.Nickname = _playersDataService.GetNickname(playerId);
            playerScoreData.PlayerDeathCount = 0;
            playerScoreData.PlayerFragCount = 0;

            PlayerScoreList.Add(playerScoreData);

            _playersScoreUI.UpdateScores(PlayerScoreList.ToArray());
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            PlayerScoreData playerScoreData = PlayerScoreList.FirstOrDefault(x => x.PlayerId == playerRef);

            PlayerScoreList.Remove(playerScoreData);
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

            for (var index = 0; index < PlayerScoreList.Count; index++)
            {
                PlayerScoreData playerScoreData = PlayerScoreList[index];
                
                if (playerScoreData.PlayerId == deadPlayer)
                {
                    var playerDeathCount = playerScoreData.PlayerDeathCount;
                    playerDeathCount++;

                    playerScoreData.PlayerDeathCount = playerDeathCount;
                }

                if (isKilledByServer == false)
                {
                    if (playerScoreData.PlayerId == killerPlayer)
                    {
                        var playerFragCount = playerScoreData.PlayerFragCount;
                        playerFragCount++;
                        
                        playerScoreData.PlayerFragCount = playerFragCount;
                    }
                }
                
                PlayerScoreList.Set(index, playerScoreData);
            }

            _playersScoreUI.UpdateScores(PlayerScoreList.ToArray());

            Debug.Log($"{killerName} killed {deadName}");
        }
    }

}