using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using DG.Tweening;
using Fusion;
using NavMeshPlus.Components;
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
        private GameStateService _gameStateService;
        private SessionStateService _sessionStateService;
        private HealthObjectsService _healthObjectsService;

        public Subject<Bot> BotSpawned { get; } = new Subject<Bot>();
        public Subject<Bot> BotDeSpawned { get; } = new Subject<Bot>();
        
        [Networked, Capacity(8)] public NetworkLinkedList<Bot> AliveBots { get; }

        protected override void Start()
        {
            base.Start();
            
            LevelService.Instance.LevelLoaded.TakeUntilDestroy(this).Subscribe((OnLevelLoaded));
        }

        [Inject]
        private void Construct(TeamsService teamsService, GameSettings gameSettings, GameStateService gameStateService, SessionStateService sessionStateService, HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
            _sessionStateService = sessionStateService;
            _gameStateService = gameStateService;
            _gameSettings = gameSettings;
            _teamsService = teamsService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();

            _gameStateService.GameRestarted.Subscribe((unit => OnGameRestarted()));
        }

        private void OnGameRestarted()
        {
            if(HasStateAuthority == false) return;

            for (var index = AliveBots.Count - 1; index >= 0; index--)
            {
                var bot = AliveBots[index];
                RespawnBot(bot);
            }
            
            SetupBots();
        }

        private void OnLevelLoaded(Level level)
        {
            if (HasStateAuthority == false) return;
            
            SetupBots();
        }

        private void SetupBots()
        {
            _levelMovePoints = LevelService.Instance.CurrentLevel.BotMovePoints;
                
            int blueBots = _gameSettings.BotsPerTeam;
            int redBots = _gameSettings.BotsPerTeam;
                
            SpawnBots(blueBots, TeamSide.Blue);
            SpawnBots(redBots, TeamSide.Red);
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
                
                bot.View.RPC_SetTeamBannerColor(AtomicConstants.Teams.GetTeamColor(team));
                
                _teamsService.AssignForTeam(bot, team);

                string id = $"{bot.GetHashCode()}";
                id = $"{id[^3]}{id[^2]}{id[^1]}";
                
                _sessionStateService.AddPlayer(bot.Object.Id, $"Bot{id}", true, team);

                _healthObjectsService.RegisterObject(bot.Object, 100);
                
                SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(bot);
                
                var botData = new BotData(sessionPlayer, CharacterClass.Engineer);
                bot.Init(botData, _levelMovePoints);
            });
                
            AliveBots.Add(bot);
            BotSpawned.OnNext(bot);
        }

        public void DespawnBot(Bot bot, bool spawnAfterDeath = true, float despawnDelay = 1) // TODO refactor, need to make pool of bots
        {
            _sessionStateService.RemovePlayer(bot.Object.Id);
            TeamSide teamSide = bot.BotData.TeamSide;
                
            BotDeSpawned.OnNext(bot);  
            
            _teamsService.RemoveFromTeam(bot);

            Observable.Timer(TimeSpan.FromSeconds(despawnDelay)).Subscribe((l =>
            {
                _healthObjectsService.RPC_UnregisterObject(bot.Object);
                
                AliveBots.Remove(bot);
                Runner.Despawn(bot.Object);
                
                if (spawnAfterDeath)
                {
                    SpawnBot(teamSide); // TODO
                }
            }));

        }

        public void RespawnBot(Bot bot)
        {   
            _healthObjectsService.RestoreHealth(bot.Object);
            
            TeamSide teamSide = bot.BotTeamSide;
            
            Vector3 spawnPos = Extensions.AtomicCart.GetSpawnPosByTeam(teamSide);

            bot.RPC_OnDeath(false);
            bot.View.transform.DOScale(1, 0.5f);
            bot.Alive = true;
            bot.transform.position = spawnPos;
        }
        
        public Bot GetBot(NetworkId id)
        {
            return AliveBots.First(x => x.Object.Id == id);
        }
        
    }
}