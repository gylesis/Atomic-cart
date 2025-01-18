using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Dev.Weapons;
using Dev.Weapons.StaticData;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

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
        private LevelService _levelService;
        private DiInjecter _diInjecter;
        private WeaponProvider _weaponProvider;

        public Subject<Bot> BotSpawned { get; } = new Subject<Bot>();
        public Subject<Bot> BotDeSpawned { get; } = new Subject<Bot>();
        
        [Networked, Capacity(8)] public NetworkLinkedList<Bot> AliveBots { get; }

        public List<BotMovePoint> LevelMovePoints => _levelMovePoints;

        [Inject]
        private void Construct(TeamsService teamsService, GameSettings gameSettings, SessionStateService sessionStateService, HealthObjectsService healthObjectsService, LevelService levelService, DiInjecter diInjecter, WeaponProvider weaponProvider)
        {
            _weaponProvider = weaponProvider;
            _diInjecter = diInjecter;
            _levelService = levelService;
            _healthObjectsService = healthObjectsService;
            _sessionStateService = sessionStateService;
            _gameSettings = gameSettings;
            _teamsService = teamsService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _levelService.LevelLoaded.Subscribe(OnLevelLoaded).AddTo(GlobalDisposable.SceneScopeToken);
            _healthObjectsService.BotDied.Subscribe(OnBotDied).AddTo(GlobalDisposable.SceneScopeToken);
        }

        private void OnBotDied(UnitDieContext context)
        {
            return;
            var bot = AliveBots.FirstOrDefault(x => x.Object.Id == context.Victim.Id);

            if (bot == null)
            {
                AtomicLogger.Err($"Not found bot with ID: {context.Victim.Id}");
                return;
            }

            // wait for animation to complete and change view
            Extensions.Delay(1, GlobalDisposable.SceneScopeToken, (() =>
            {
                var characterClass = (CharacterClass)Random.Range(0, Enum.GetNames(typeof(CharacterClass)).Length);
                var characterData = _gameSettings.CharactersDataContainer.GetCharacterDataByClass(characterClass);
                bot.View.UpdateCharacterView(characterData.AnimatorController, characterData.CharacterSprite);
                _weaponProvider.ProvideWeapon(Runner, bot.WeaponController, characterData.WeaponType, true);
            }));
        }

        protected override void CorrectState()
        {
            base.CorrectState();

            if (_levelService.CurrentLevel != null) 
                _levelMovePoints = _levelService.CurrentLevel.BotMovePoints;
        }

        private void OnLevelLoaded(Level level)
        {
            if (Runner.IsSharedModeMasterClient == false) return;
            
            _levelMovePoints = _levelService.CurrentLevel.BotMovePoints;

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

                _diInjecter.InjectGameObject(bot.gameObject);
                _teamsService.RPC_AssignForTeam(new TeamMember(bot), team);
                var characterClass = (CharacterClass)Random.Range(0, Enum.GetNames(typeof(CharacterClass)).Length);

                string id = $"{bot.GetHashCode()}";
                id = $"{id[^4]}{id[^3]}{id[^2]}{id[^1]}";
                
                _sessionStateService.AddPlayer(bot.Object.Id, $"Bot{id}", true);
                _healthObjectsService.RegisterObject(bot.Object.Id, 100);
                
                SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(bot);
                
                var botData = new BotData(sessionPlayer, characterClass);
                bot.Init(botData, team);

                var characterData = _gameSettings.CharactersDataContainer.GetCharacterDataByClass(characterClass);
                bot.View.UpdateCharacterView(characterData.AnimatorController, characterData.CharacterSprite);

                _weaponProvider.ProvideWeapon(Runner, bot.WeaponController, characterData.WeaponType, true);
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
            
            var characterClass = (CharacterClass)Random.Range(0, Enum.GetNames(typeof(CharacterClass)).Length);
            var characterData = _gameSettings.CharactersDataContainer.GetCharacterDataByClass(characterClass);
            bot.View.UpdateCharacterView(characterData.AnimatorController, characterData.CharacterSprite);
            _weaponProvider.ProvideWeapon(Runner, bot.WeaponController, characterData.WeaponType, true);
            
            bot.RPC_OnDeath(false);
        }

        public void DespawnBot(Bot bot, bool spawnAfterDeath = true, float despawnDelay = 1) // TODO refactor, need to make pool of bots
        {
            _sessionStateService.RemovePlayer(bot.Object.Id);
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
                _healthObjectsService.UnRegisterObject(bot.Object.Id);
                
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