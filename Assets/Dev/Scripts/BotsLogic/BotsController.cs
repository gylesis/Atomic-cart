using System;
using System.Collections.Generic;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.BotsLogic
{
    public class BotsController : NetworkContext
    {
        [SerializeField] private Bot _botPrefab;
        
        private List<BotMovePoint> _levelMovePoints;

        private TeamsService _teamsService;
        private GameSettings _gameSettings;

        public Subject<Bot> BotSpawned { get; } = new Subject<Bot>();
        public Subject<Bot> BotDeSpawned { get; } = new Subject<Bot>();
        
        [Networked, Capacity(8)] public NetworkLinkedList<Bot> AliveBots { get; }
        
        private void Start()
        {
            LevelService.Instance.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
        }
        
        [Inject]
        private void Construct(TeamsService teamsService, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _teamsService = teamsService;
        }
        
        private void OnLevelLoaded(Level level) 
        {
            if (HasStateAuthority)
            {
                _levelMovePoints = level.BotMovePoints;
                
                int blueBots = _gameSettings.BotsPerTeam;
                int redBots = _gameSettings.BotsPerTeam;
                
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
                bot.Init(botData, _levelMovePoints);
            });
                
            AliveBots.Add(bot);
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