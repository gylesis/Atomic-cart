using System;
using System.Linq;
using System.Threading.Tasks;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Utils;
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
            base.Spawned();
            
            _playersDataService.PlayersSpawner.BaseSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            _playersDataService.PlayersSpawner.BaseDespawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));

            _botsController.BotSpawned.TakeUntilDestroy(this).Subscribe((OnBotSpawned));
            _botsController.BotDeSpawned.TakeUntilDestroy(this).Subscribe((OnBotDespawned));
            
            _healthObjectsService.PlayerDied.TakeUntilDestroy(this).Subscribe(OnUnitDied);
            _healthObjectsService.BotDied.TakeUntilDestroy(this).Subscribe(OnUnitDied);
        }

        private void OnUnitDied(UnitDieContext context)
        {
            if(HasStateAuthority == false) return;
             
            SessionPlayer victim = context.Victim;
            TrySetScoreData(victim, (data =>
            {
                data.PlayerDeathCount += 1;
                return data;
            }));

            if (context.IsKilledByServer == false)
            {
                TrySetScoreData(context.Killer, (data =>
                {
                    data.PlayerFragCount += 1;
                    return data;
                }));
                
                SaveLoadService.Instance.AddKill(context.Killer);
            }
            
            RPC_UpdateScore();
        }

        private bool TrySetScoreData(SessionPlayer sessionPlayer, Func<PlayerScoreData, PlayerScoreData> action)
        { 
            if(PlayerScoreList.Any(x => x.SessionPlayer.Id == sessionPlayer.Id) == false) return false;
            
            PlayerScoreData playerScoreData = PlayerScoreList.FirstOrDefault(x => x.SessionPlayer.Id == sessionPlayer.Id);
            int index = PlayerScoreList.IndexOf(playerScoreData);

            playerScoreData = action(playerScoreData);
            PlayerScoreList.Set(index, playerScoreData);
            
            return true;
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

            SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(playerRef.ToNetworkId());
            var playerScoreData = new PlayerScoreData(sessionPlayer);

            playerScoreData.PlayerDeathCount = 0;
            playerScoreData.PlayerFragCount = 0;

            PlayerScoreList.Add(playerScoreData);

            RPC_UpdateScore();
            // _playersScoreUI.UpdateScores(PlayerScoreList.ToArray());
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            if(Runner.IsSharedModeMasterClient == false) return;
            
            NetworkId id = playerRef.ToNetworkId();
            
            PlayerScoreData playerScoreData = PlayerScoreList.FirstOrDefault(x => x.SessionPlayer.Id == id);

            Debug.Log($"Remove player {playerRef}");
            PlayerScoreList.Remove(playerScoreData);

            RPC_UpdateScore();
        }

        [Rpc]
        private void RPC_UpdateScore()
        {
            OnScoreUpdate.OnNext(Unit.Default);
        }
    }

}