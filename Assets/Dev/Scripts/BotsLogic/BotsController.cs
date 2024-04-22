using System;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.BotsLogic
{
    public class BotsController : NetworkContext
    {
        [SerializeField] private Bot _botPrefab;
        
        private TeamsService _teamsService;

        public Subject<Bot> BotSpawned { get; } = new Subject<Bot>();
        public Subject<Bot> BotDeSpawned { get; } = new Subject<Bot>();
        
        private void Start()
        {
            LevelService.Instance.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
        }
        
        [Inject]
        private void Construct(TeamsService teamsService)
        {
            _teamsService = teamsService;
        }
        
        private void OnLevelLoaded(Level level) 
        {
            int blueBots = 2;
            int redBots = 2;

            if (HasStateAuthority)
            {
                SpawnBots(blueBots, TeamSide.Blue);
                SpawnBots(redBots, TeamSide.Red);
            }   
        }
        
        private void SpawnBots(int botsCount, TeamSide team)
        {
            for (int i = 0; i < botsCount; i++)
            {
                SpawnBot(team);
            }
        }

        public void SpawnBot(TeamSide team)
        {
            Vector3 spawnPos = Extensions.AtomicCart.GetSpawnPosByTeam(team);
    
            Bot bot = Runner.Spawn(_botPrefab, spawnPos, onBeforeSpawned: (runner, o) =>
            {
                var bot = o.GetComponent<Bot>();

                DependenciesContainer.Instance.Inject(bot.gameObject);
                
                var botData = new BotData();
                botData.TeamSide = team;
                botData.CharacterClass = CharacterClass.Engineer;
                
                bot.View.RPC_SetTeamBannerColor(AtomicConstants.Teams.GetTeamColor(team));
                
                _teamsService.AssignForTeam(bot, team);
                bot.Init(botData);
            });
                
            BotSpawned.OnNext(bot);
        }

        public void DespawnBot(Bot bot, bool spawnAfterDeath = true)
        {
            TeamSide teamSide = bot.BotData.TeamSide;
            
            BotDeSpawned.OnNext(bot);  
            
            _teamsService.RemoveFromTeam(bot);

            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe((l =>
            {
                Runner.Despawn(bot.Object);

                if (spawnAfterDeath)
                {
                    SpawnBot(teamSide); // TODO
                }
            }));

        }
        
    }
}