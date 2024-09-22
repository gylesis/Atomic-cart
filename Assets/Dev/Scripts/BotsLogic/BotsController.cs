using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Fusion;
using NavMeshPlus.Components;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
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

        public List<BotMovePoint> LevelMovePoints => _levelMovePoints;

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
            if (Runner.IsSharedModeMasterClient == false) return;

            for (var index = AliveBots.Count - 1; index >= 0; index--)
            {
                var bot = AliveBots[index];
                RPC_RespawnBot(bot);
            }
            
            SetupBots();
        }

        protected override void CorrectState()
        {
            base.CorrectState();

            if (LevelService.Instance.CurrentLevel != null)
            {
                _levelMovePoints = LevelService.Instance.CurrentLevel.BotMovePoints;
            }
        }

        private void OnLevelLoaded(Level level)
        {
            if (Runner.IsSharedModeMasterClient == false) return;
            
            _levelMovePoints = LevelService.Instance.CurrentLevel.BotMovePoints;
            
            SetupBots();
        }

        private void SetupBots()
        {
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

                DiInjecter.Instance.InjectGameObject(bot.gameObject);
                
                bot.View.RPC_SetTeamBannerColor(AtomicConstants.Teams.GetTeamColor(team));
                
                _teamsService.RPC_AssignForTeam(bot, team);

                string id = $"{bot.GetHashCode().ToString().PadRight(3)}";
                
                _sessionStateService.RPC_AddPlayer(bot.Object.Id, $"Bot{id}", true, team);

                _healthObjectsService.RegisterObject(bot.Object, 100);
                
                SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(bot);
                
                var botData = new BotData(sessionPlayer, CharacterClass.Engineer);
                bot.Init(botData);
            });
                
            AliveBots.Add(bot);
            BotSpawned.OnNext(bot);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RespawnBot(Bot bot)
        {   
            _healthObjectsService.RestoreHealth(bot.Object);
            
            TeamSide teamSide = bot.BotTeamSide;
            
            Vector3 spawnPos = Extensions.AtomicCart.GetSpawnPosByTeam(teamSide);
           
            bot.NavMeshAgent.nextPosition = spawnPos;
            bot.transform.position = spawnPos;
            bot.NavMeshAgent.ResetPath();
            
            bot.RPC_OnDeath(false);
        }

        public void DespawnBot(Bot bot, bool spawnAfterDeath = true, float despawnDelay = 1) // TODO refactor, need to make pool of bots
        {
            _sessionStateService.RPC_RemovePlayer(bot.Object.Id);
            TeamSide teamSide = bot.BotData.TeamSide;
                
            BotDeSpawned.OnNext(bot);  
            
            _teamsService.RPC_RemoveFromTeam(bot);

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

        public Bot GetBot(NetworkId id)
        {   
            return AliveBots.First(x => x.Object.Id == id);
        }
                
    }
}