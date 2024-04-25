using System;
using System.Linq;
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
    public class BotsHealthService : NetworkContext
    {
        [Networked, Capacity(20)] private NetworkDictionary<int, int> BotsHealth { get; }
    
        public static BotsHealthService Instance { get; private set; }

        private TeamsService _teamsService;
        private WorldTextProvider _worldTextProvider;
        private CharactersDataContainer _charactersDataContainer;
        
        private BotsController _botsController;
        private GameSettings _gameSettings;
        private HealthObjectsService _healthObjectsService;

        public Subject<BotDieContext> BotKilled { get; } = new Subject<BotDieContext>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        [Inject]
        private void Construct(BotsController botsController, GameStaticDataContainer gameStaticDataContainer, GameSettings gameSettings, TeamsService teamsService, WorldTextProvider worldTextProvider, HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
            _worldTextProvider = worldTextProvider;
            _teamsService = teamsService;
            _gameSettings = gameSettings;
            _charactersDataContainer = gameStaticDataContainer.CharactersDataContainer;
            _botsController = botsController;
        }

        public override void Spawned()
        {
            if (HasStateAuthority == false) return;

            _botsController.BotSpawned.TakeUntilDestroy(this).Subscribe((OnBotSpawned));
            _botsController.BotDeSpawned.TakeUntilDestroy(this).Subscribe((OnBotDeSpawned));
        }

        private void OnBotSpawned(Bot bot)  
        {
            int startHealth = _charactersDataContainer.GetCharacterDataByClass(bot.BotData.CharacterClass)
                .CharacterStats.Health;
            
            if (Runner.IsSharedModeMasterClient)
            {
                _healthObjectsService.RegisterObject(bot.Object, startHealth);
            }
            
            BotsHealth.Add(bot.Object.Id.Raw.GetHashCode(), startHealth);
        }

        private void OnBotDeSpawned(Bot bot)  
        {
            if (Runner.IsSharedModeMasterClient)
            {
                _healthObjectsService.UnregisterObject(bot.Object);
            }
            
            BotsHealth.Remove(bot);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ApplyDamageToBotFromClient(Bot victim, PlayerRef shooter, int damage)
        {
            ApplyDamageToBot(victim, shooter, damage);
        }
        
        public void ApplyDamageToBot(Bot victim, PlayerRef shooter, int damage)
        {
            if (_gameSettings.IsFriendlyFireOn == false)
            {
                TeamSide victimTeamSide = _teamsService.GetUnitTeamSide(victim);
                TeamSide shooterTeamSide = _teamsService.GetUnitTeamSide(shooter);

                if (victimTeamSide == shooterTeamSide) return;
            }

            int currentHealth = BotsHealth[victim];

            if (currentHealth == 0) return;

            //var nickname = PlayersDataService.Instance.GetNickname(victim);
            var nickname = $"Bot {victim.Object.Id}";

            Debug.Log($"Damage {damage} applied to bot {nickname}", victim);
            //LoggerUI.Instance.Log($"Damage {damage} applied to player {nickname}");

            RPC_SpawnDamageHintFor(shooter, victim.transform.position, damage);

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnHealthZero(victim, shooter);
            }

            Debug.Log($"{nickname} has {currentHealth} health");

            RPC_UpdatePlayerHealth(victim, currentHealth);
        }

        private void OnHealthZero(Bot bot, PlayerRef shooter)
        {
            _botsController.DespawnBot(bot);

            var botDieContext = new BotDieContext();
            botDieContext.Killer = shooter;
            
            bot.Alive = false;
            bot.View.transform.DOScale(0, 0.5f);
            
            BotKilled.OnNext(botDieContext);

            //LoggerUI.Instance.Log($"Player {playerRef} is dead");

            float respawnTime = 2;
            
            Observable.Timer(TimeSpan.FromSeconds(respawnTime)).Subscribe((l =>
            {
               // _playersSpawner.RespawnPlayerCharacter(bot);
            }));
        }

        public void RestoreFullHealth(Bot bot)
        {
            CharacterData characterData = _charactersDataContainer.GetCharacterDataByClass(bot.BotData.CharacterClass);

            int maxHealth = characterData.CharacterStats.Health;

            GainHealth(bot, maxHealth);
        }

        public void GainHealth(Bot bot, int health)
        {
            Debug.Log($"Gained {health} HP for player {bot}");

            CharacterData characterData = _charactersDataContainer.GetCharacterDataByClass(bot.BotData.CharacterClass);

            int maxHealth = characterData.CharacterStats.Health;
            
            int currentHealth = BotsHealth[bot];
            currentHealth += health;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            RPC_UpdatePlayerHealth(bot, currentHealth);
        }

        [Rpc]
        private void RPC_SpawnDamageHintFor([RpcTarget] PlayerRef playerRef, Vector3 pos, int damage)
        {
            _worldTextProvider.SpawnDamageText(pos, damage);
        }

        [Rpc]
        private void RPC_UpdatePlayerHealth(Bot bot, int health)
        {
            BotsHealth.Set(bot, health);    
        }
    }


    public struct BotDieContext
    {
        public PlayerRef Killer;
    }
    
}