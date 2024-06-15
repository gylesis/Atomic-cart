using System;
using System.Linq;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons.Guns;
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
            
            if(Runner.IsSharedModeMasterClient == false) return;
            
            bool hasObject = HealthData.Any(x => x.ObjId == networkObject.Id);

            if (hasObject)
            {
                ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == networkObject.Id);

                HealthData.Remove(healthData);
            }
        }

        public void ApplyDamage(ApplyDamageContext damageContext)
        {
            NetworkObject victimObj = damageContext.VictimObj;
            TeamSide shooterTeam = damageContext.ShooterTeam;
            int damage = damageContext.Damage;
            PlayerRef shooter = damageContext.Shooter;

            bool isDamagable = victimObj.TryGetComponent<IDamageable>(out var damagable);

            if (isDamagable == false)
            {
                Debug.Log($"Object {victimObj.name} is not damagable, skipping {damage} damage", victimObj);
                return;
            }
          
            NetworkId victimId = victimObj.Id;
            PlayerRef victimPlayerRef = victimObj.StateAuthority;

            bool hasHealthDataForObject = HealthData.Any(x => x.ObjId == victimId);

            if (hasHealthDataForObject == false)
            {
                Debug.Log($" No Health Data presented for object {victimObj.name}, skipping {damage} damage. Probably missed initialization", victimObj);
                return;
            }
            
            // target
            bool isDummyTarget = damagable.DamageId == DamagableType.DummyTarget;
            bool isBot = damagable.DamageId == DamagableType.Bot;
            bool isStaticObstacle = damagable.DamageId == DamagableType.Obstacle;
            bool isObstacleWithHealth = damagable.DamageId == DamagableType.ObstacleWithHealth;
            bool isPlayer = damagable.DamageId == DamagableType.Player;

            if (isPlayer || isBot)
            {
                if (_gameSettings.IsFriendlyFireOn == false)
                {
                    TeamSide victimTeamSide ;

                    if (isBot)
                    {
                        victimTeamSide = _teamsService.GetUnitTeamSide(victimId);
                    }
                    else 
                    {
                        victimTeamSide = _teamsService.GetUnitTeamSide(victimObj.StateAuthority);
                    }
                    
                    if (victimTeamSide == shooterTeam) return;
                }
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

            if (isPlayer)
            {
                PlayerCharacter playerCharacter = damagable as PlayerCharacter;

                PlayerRef victim = victimObj.StateAuthority;
                
                RPC_SpawnDamageHintFor(victim, victimObj.transform.position, damage);

                string nickname = _playersDataService.GetNickname(victimPlayerRef);

                if (currentHealth == 0)
                {
                    OnPlayerHealthZero(playerCharacter.Object.StateAuthority, shooter);
                }
                
                Debug.Log($"Player {nickname} got hit for {damage}");
            }

            if (isBot)
            {
                Bot bot = damagable as Bot; 

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

            Debug.Log($"Damage {damage} applied to {victimObj.name}", victimObj);
            
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
        
        public void RestoreHealth(NetworkObject networkObject, bool isPlayer)
        {
            if (isPlayer)
            {
                PlayerRef playerRef = networkObject.StateAuthority;
                
                CharacterClass playerCharacterClass = _playersDataService.GetPlayerCharacterClass(playerRef);
                
                CharacterData characterData = _gameStaticDataContainer.CharactersDataContainer.GetCharacterDataByClass(playerCharacterClass);
             
                GainHealthToPlayer(networkObject, characterData.CharacterStats.Health);
            }
        }

        public void GainHealthToPlayer(NetworkObject player, int health)
        {
            PlayerRef playerRef = player.StateAuthority;   
            NetworkId playerId = player.Id;
            
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
            PlayerBase playerBase = _playersDataService.GetPlayerBase(victim);

            playerCharacter.RPC_OnDeath();
            
            playerBase.PlayerController.SetAllowToMove(false);
            playerBase.PlayerController.SetAllowToShoot(false);
           
            var playerDieEventContext = new PlayerDieEventContext();
            playerDieEventContext.Killer = killer;
            playerDieEventContext.Killed = victim;

            PlayerKilled.OnNext(playerDieEventContext);

            //LoggerUI.Instance.Log($"Player {playerRef} is dead");

            float respawnTime = 2;
            
            Observable.Timer(TimeSpan.FromSeconds(respawnTime)).Subscribe((l =>
            {
                RestoreHealth(playerCharacter.Object, true);
                _playersDataService.PlayersSpawner.RespawnPlayerCharacter(victim);
            }));
        }
        
        
        [Rpc]
        private void RPC_SpawnDamageHintFor([RpcTarget] PlayerRef playerRef, Vector3 pos, int damage)
        {
            _worldTextProvider.SpawnDamageText(pos, damage);
        }
        
    }

    public struct ApplyDamageContext
    {
        public int Damage;
        public PlayerRef Shooter;
        public TeamSide ShooterTeam;
        public NetworkObject VictimObj;
    }
    
}