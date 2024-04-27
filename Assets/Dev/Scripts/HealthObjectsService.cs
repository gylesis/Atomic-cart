using System;
using System.Linq;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class HealthObjectsService : NetworkContext
    {
        private GameSettings _gameSettings;
        private TeamsService _teamsService;
        private WorldTextProvider _worldTextProvider;
        private PlayersDataService _playersDataService;
        private GameStaticDataContainer _gameStaticDataContainer;
        private BotsController _botsController;

        [Networked, Capacity(128)] private NetworkLinkedList<ObjectWithHealthData> HealthData { get; }

        public Subject<ObjectWithHealthData> HealthChanged { get; } = new Subject<ObjectWithHealthData>();
        public Subject<ObjectWithHealthData> HealthZero { get; } = new Subject<ObjectWithHealthData>();

        public Subject<PlayerDieEventContext> PlayerKilled { get; } = new Subject<PlayerDieEventContext>();
        public Subject<BotDieContext> BotKilled { get; } = new Subject<BotDieContext>();

        
        [Inject]
        private void Construct(GameSettings gameSettings, TeamsService teamsService, WorldTextProvider worldTextProvider, PlayersDataService playersDataService, GameStaticDataContainer gameStaticDataContainer, BotsController botsController)
        {
            _botsController = botsController;
            _gameStaticDataContainer = gameStaticDataContainer;
            _playersDataService = playersDataService;
            _worldTextProvider = worldTextProvider;
            _teamsService = teamsService;
            _gameSettings = gameSettings;
        }
        
        public void RegisterObject(NetworkObject networkObject, int health)
        {
            health = Mathf.Clamp(health, 0, UInt16.MaxValue); // to avoid uint overflow
            
            ObjectWithHealthData healthData = new ObjectWithHealthData(networkObject.Id, (UInt16) health);

            HealthData.Add(healthData);
        }   

        public void UnregisterObject(NetworkObject networkObject)
        {
            if(Runner == null) return;
            
            if(Runner.IsSharedModeMasterClient)
            {
                if (Runner.ActivePlayers.Count() == 1)
                {
                    return;
                }
            }
            else
            {
                return;
            }
            
            bool hasObject = HealthData.Any(x => x.ObjId == networkObject.Id);

            if (hasObject)
            {
                ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == networkObject.Id);

                HealthData.Remove(healthData);
            }
        }

        public void ApplyDamage(NetworkObject victimObj, PlayerRef shooter, int damage)
        {
            if (victimObj.TryGetComponent<ObjectWithHealth>(out var objectWithHealth) == false)
            {
                Debug.Log($"Object {victimObj.name} not object with health, please add health <ObjectWithHealth> component", victimObj.gameObject);
                return; 
            }
            
            NetworkId victimId = victimObj.Id;
            PlayerRef victimPlayerRef = victimObj.StateAuthority;

            bool hasObject = HealthData.Any(x => x.ObjId == victimId);

            if (hasObject == false) return;
            
            if (_gameSettings.IsFriendlyFireOn == false)
            {
                TeamSide victimTeamSide = _teamsService.GetUnitTeamSide(victimId);
                TeamSide shooterTeamSide = _teamsService.GetUnitTeamSide(shooter);

                if (victimTeamSide == shooterTeamSide) return;
            }

            RPC_SpawnDamageHintFor(shooter, victimObj.transform.position, damage);
            
            bool isDummy = victimObj.TryGetComponent<DummyTarget>(out var dummyTarget);

            if (isDummy)
            {
                Debug.Log($"Damage {damage} applied to dummy target {dummyTarget.name}");

                Vector3 playerPos = dummyTarget.transform.position;

                damage = 0;
            }
            
            int currentHealth = ApplyDamageInternal(victimObj, damage);

            bool isPlayer = victimObj.TryGetComponent<PlayerCharacter>(out var playerCharacter);
            
            if (isPlayer)
            {
                //Vector3 playerPos = _playersDataService.GetPlayerPos(victimPlayerRef);
                
                //RPC_SpawnDamageHintFor(shooter, playerPos, damage);
                //RPC_SpawnDamageHintFor(victim, playerPos, damage);

                string nickname = _playersDataService.GetNickname(victimPlayerRef);

                if (currentHealth == 0)
                {
                    OnPlayerHealthZero(playerCharacter.Object.StateAuthority, shooter);
                }
                
                Debug.Log($"Player {nickname} got hit for {damage}");
            }

            bool isBot = victimObj.TryGetComponent<Bot>(out var bot);

            if (isBot)
            {
                if (currentHealth == 0)
                {
                    OnBotHealthZero(bot, shooter);
                }
                
            }

           

            
        }
    
        private int ApplyDamageInternal(NetworkObject victimObj, int damage)
        {
            NetworkId victimId = victimObj.Id;      
            damage = Mathf.Clamp(damage, 0, UInt16.MaxValue); // to avoid uint overflow

            ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == victimId);
            int index = HealthData.IndexOf(healthData);  

            int currentHealth = healthData.Health;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, UInt16.MaxValue);

            ObjectWithHealthData data = new ObjectWithHealthData(victimId, (UInt16)currentHealth);

            HealthData.Set(index, data);

            HealthChanged.OnNext(data);

            if (currentHealth <= 0)
            {
                HealthZero.OnNext(data);
            }

            return currentHealth;
        }
        
        private void OnBotHealthZero(Bot bot, PlayerRef shooter)
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
        
        public void RestoreHealth(ObjectWithHealth objectWithHealth, bool isPlayer)
        {
            if (isPlayer)
            {
                PlayerRef playerRef = objectWithHealth.Object.StateAuthority;
                
                CharacterClass playerCharacterClass = _playersDataService.GetPlayerCharacterClass(playerRef);
                
                CharacterData characterData = _gameStaticDataContainer.CharactersDataContainer.GetCharacterDataByClass(playerCharacterClass);
             
                GainHealthToPlayer(objectWithHealth, characterData.CharacterStats.Health);
            }
        }

        public void GainHealthToPlayer(ObjectWithHealth objectWithHealth, int health)
        {
            PlayerRef playerRef = objectWithHealth.Object.StateAuthority;   
            NetworkId playerId = objectWithHealth.Object.Id;
            
            CharacterClass playerCharacterClass = _playersDataService.GetPlayerCharacterClass(playerRef);

            CharacterData characterData = _gameStaticDataContainer.CharactersDataContainer.GetCharacterDataByClass(playerCharacterClass);

            int maxHealth = characterData.CharacterStats.Health;
            
            ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == playerId);
            int index = HealthData.IndexOf(healthData);  
            
            int currentHealth = healthData.Health;
            currentHealth += health;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            
            ObjectWithHealthData data = new ObjectWithHealthData(playerId, (UInt16)currentHealth);
            HealthData.Set(index, data);
            
            Debug.Log($"Gained {health} HP for player {playerRef}");
        }
        
        private void OnPlayerHealthZero(PlayerRef victim, PlayerRef killer)
        {
            PlayerCharacter playerCharacter = _playersDataService.GetPlayer(victim);

            playerCharacter.RPC_OnDeath();
            
            playerCharacter.PlayerController.SetAllowToMove(false);
            playerCharacter.PlayerController.SetAllowToShoot(false);
           
            var playerDieEventContext = new PlayerDieEventContext();
            playerDieEventContext.Killer = killer;
            playerDieEventContext.Killed = victim;

            PlayerKilled.OnNext(playerDieEventContext);

            //LoggerUI.Instance.Log($"Player {playerRef} is dead");

            float respawnTime = 2;
            
            Observable.Timer(TimeSpan.FromSeconds(respawnTime)).Subscribe((l =>
            {
                _playersDataService.PlayersSpawner.RespawnPlayerCharacter(victim);
            }));
        }
        
        
        [Rpc]
        private void RPC_SpawnDamageHintFor([RpcTarget] PlayerRef playerRef, Vector3 pos, int damage)
        {
            _worldTextProvider.SpawnDamageText(pos, damage);
        }
        
    }
}