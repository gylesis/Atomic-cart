using System.Linq;
using System.Threading.Tasks;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersScoreService : NetworkContext
    {
        [Networked, Capacity(10)] public NetworkLinkedList<PlayerScoreData> PlayerScoreList { get; }
       
        private PlayersDataService _playersDataService;
        private SessionStateService _sessionStateService;
        private BotsController _botsController;
        private HealthObjectsService _healthObjectsService;

        public Subject<Unit> OnScoreUpdate { get; } = new Subject<Unit>();
        
        [Inject]
        private void Init(TeamsService teamsService, PlayersDataService playersDataService, SessionStateService sessionStateService, BotsController botsController, HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
            _botsController = botsController;
            _sessionStateService = sessionStateService;
            _playersDataService = playersDataService;
        }

        public override void Spawned()
        {
            _playersDataService.PlayersSpawner.PlayerBaseSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            _playersDataService.PlayersSpawner.PlayerBaseDeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));

            _botsController.BotSpawned.TakeUntilDestroy(this).Subscribe((OnBotSpawned));
            _botsController.BotDeSpawned.TakeUntilDestroy(this).Subscribe((OnBotDespawned));
            
            _healthObjectsService.PlayerDied.TakeUntilDestroy(this).Subscribe(UpdateTableScore);
            _healthObjectsService.BotDied.TakeUntilDestroy(this).Subscribe(UpdateTableScore);
        }

        private void OnBotSpawned(Bot bot)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            PlayerScoreList.Add(new PlayerScoreData(_sessionStateService.GetSessionPlayer(bot)));
            
            RPC_UpdateScore();
        }

        private void OnBotDespawned(Bot bot)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            if (PlayerScoreList.Any(x => x.SessionPlayer.Id == bot.Object.Id))
            {
                PlayerScoreList.Remove(PlayerScoreList.First(x => x.SessionPlayer.Id == bot.Object.Id));
            }

            RPC_UpdateScore();
        }

        private async void OnPlayerSpawned(PlayerSpawnEventContext playerSpawnData)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            await Task.Delay(500);
            
            PlayerRef playerRef = playerSpawnData.PlayerRef;

            NetworkId id = _playersDataService.GetPlayerBase(playerRef).Object.Id;

            var playerScoreData = new PlayerScoreData(_sessionStateService.GetSessionPlayer(id));

            playerScoreData.PlayerDeathCount = 0;
            playerScoreData.PlayerFragCount = 0;

            PlayerScoreList.Add(playerScoreData);

            RPC_UpdateScore();
            // _playersScoreUI.UpdateScores(PlayerScoreList.ToArray());
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            NetworkId id = _playersDataService.GetPlayerBase(playerRef).Object.Id;
            
            PlayerScoreData playerScoreData = PlayerScoreList.FirstOrDefault(x => x.SessionPlayer.Id == id);

            PlayerScoreList.Remove(playerScoreData);

            RPC_UpdateScore();
        }

        private void UpdateTableScore(UnitDieContext context)
        {
            SessionPlayer killerPlayer = context.Killer;
            SessionPlayer deadPlayer = context.Victim;
            
            bool killedByServer = context.IsKilledByServer;
            
            string killerName;

            if (killedByServer)
            {
                killerName = "Server";
            }
            else
            {
                killerName = killerPlayer.Name;
            }
            
            string deadName = deadPlayer.Name;

            for (var index = 0; index < PlayerScoreList.Count; index++)
            {
                PlayerScoreData playerScoreData = PlayerScoreList[index];
                
                if (playerScoreData.SessionPlayer.Id == deadPlayer.Id)
                {
                    var playerDeathCount = playerScoreData.PlayerDeathCount;
                    playerDeathCount++;

                    playerScoreData.PlayerDeathCount = playerDeathCount;
                }

                if (killedByServer == false)
                {
                    if (playerScoreData.SessionPlayer.Id == killerPlayer.Id)
                    {
                        var playerFragCount = playerScoreData.PlayerFragCount;
                        playerFragCount++;
                        
                        playerScoreData.PlayerFragCount = playerFragCount;
                    }
                }
                
                PlayerScoreList.Set(index, playerScoreData);
            }
            
            Debug.Log($"{killerName} killed {deadName}");

            RPC_UpdateScore();
        }

        [Rpc]
        private void RPC_UpdateScore()
        {
            OnScoreUpdate.OnNext(Unit.Default);
        }
    }

}