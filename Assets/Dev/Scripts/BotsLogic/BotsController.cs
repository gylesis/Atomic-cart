using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
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
        private void Construct(TeamsService teamsService, GameSettings gameSettings, SessionStateService sessionStateService, HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
            _sessionStateService = sessionStateService;
            _gameSettings = gameSettings;
            _teamsService = teamsService;
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
            int blueBots = _gameSettings.BotsConfig.BotsPerTeam;
            int redBots = _gameSettings.BotsConfig.BotsPerTeam;
                
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
                
                _teamsService.RPC_AssignForTeam(new TeamMember(bot), team);

                string id = $"{bot.GetHashCode()}";
                id = $"{id[^4]}{id[^3]}{id[^2]}{id[^1]}";
                
                _sessionStateService.RPC_AddPlayer(bot.Object.Id, $"Bot{id}", true);
                _healthObjectsService.RegisterObject(bot.Object, 100);
                
                SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(bot);
                
                var botData = new BotData(sessionPlayer, CharacterClass.Engineer);
                bot.Init(botData, team);
            });
                
            AliveBots.Add(bot);
            BotSpawned.OnNext(bot);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RespawnBot(Bot bot)
        {   
            _healthObjectsService.RestoreHealth(bot.Object);
            
            TeamSide teamSide = bot.GetTeamSide();
            
            Vector3 spawnPos = Extensions.AtomicCart.GetSpawnPosByTeam(teamSide);
           
            bot.NavMeshAgent.nextPosition = spawnPos;
            bot.transform.position = spawnPos;
            bot.NavMeshAgent.ResetPath();
            
            bot.RPC_OnDeath(false);
        }

        public void DespawnBot(Bot bot, bool spawnAfterDeath = true, float despawnDelay = 1) // TODO refactor, need to make pool of bots
        {
            _sessionStateService.RPC_RemovePlayer(bot.Object.Id);
            var hasTeam = _sessionStateService.TryGetPlayerTeam(bot.BotData.SessionPlayer, out var teamSide);

            if (!hasTeam)
            {
                AtomicLogger.Err(hasTeam.ErrorMessage);
                return;
            }

            BotDeSpawned.OnNext(bot);  
            
            _teamsService.RPC_RemoveFromTeam(bot.Object);

            Extensions.Delay(despawnDelay, destroyCancellationToken, () =>
            {
                _healthObjectsService.RPC_UnregisterObject(bot.Object);
                
                AliveBots.Remove(bot);
                Runner.Despawn(bot.Object);
                
                if (spawnAfterDeath)
                {
                    SpawnBot(teamSide); // TODO
                } 
            });
        }

        public Bot GetBot(NetworkId id)
        {   
            return AliveBots.First(x => x.Object.Id == id);
        }
                
    }
}