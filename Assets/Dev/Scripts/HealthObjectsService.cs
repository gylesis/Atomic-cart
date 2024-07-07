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
        private SessionStateService _sessionStateService;

        [Networked, Capacity(128)] private NetworkLinkedList<ObjectWithHealthData> HealthData { get; }

        public Subject<ObjectWithHealthData> HealthChanged { get; } = new Subject<ObjectWithHealthData>();
        public Subject<ObjectWithHealthData> HealthZero { get; } = new Subject<ObjectWithHealthData>();

        public Subject<UnitDieContext> PlayerDied { get; } = new Subject<UnitDieContext>();
        public Subject<UnitDieContext> BotDied { get; } = new Subject<UnitDieContext>();


        [Inject]
        private void Construct(GameSettings gameSettings, TeamsService teamsService,
                               WorldTextProvider worldTextProvider, PlayersDataService playersDataService,
                               GameStaticDataContainer gameStaticDataContainer, BotsController botsController,
                               SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
            _botsController = botsController;
            _gameStaticDataContainer = gameStaticDataContainer;
            _playersDataService = playersDataService;
            _worldTextProvider = worldTextProvider;
            _teamsService = teamsService;
            _gameSettings = gameSettings;
        }

        public void RegisterObject(NetworkObject networkObject, int health)
        {
            if (Runner.IsSharedModeMasterClient == false) return;

            RPC_InternalRegisterObject(networkObject, health);
        }

        public void RegisterPlayer(PlayerCharacter playerCharacter)
        {
            if (Runner.IsSharedModeMasterClient == false) return;
                
            PlayerRef playerRef = playerCharacter.Object.InputAuthority;

            CharacterClass playerCharacterClass = _playersDataService.GetPlayerCharacterClass(playerRef);

            CharacterData characterData =
                _gameStaticDataContainer.CharactersDataContainer.GetCharacterDataByClass(playerCharacterClass);

            RPC_InternalRegisterObject(playerCharacter.Object, characterData.CharacterStats.Health);
        }
            

        [Rpc]
        private void RPC_InternalRegisterObject(NetworkObject networkObject, int health)
        {
            if (HasData(networkObject.Id))
            {
                Debug.Log($"Trying to register already registered object", networkObject);
                return;
            }

            health = Mathf.Clamp(health, 0, UInt16.MaxValue); // to avoid uint overflow

            ObjectWithHealthData healthData = new ObjectWithHealthData(networkObject.Id, (UInt16)health, (UInt16)health);

            HealthData.Add(healthData);

            Debug.Log($"Registering health object {networkObject.name}, total count {HealthData.Count}", networkObject);
        }

        [Rpc]
        public void RPC_UnregisterObject(NetworkObject networkObject)
        {
            if (Runner == null) return;

            if (Runner.IsSharedModeMasterClient == false) return;

            if (HasData(networkObject.Id))
            {
                ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == networkObject.Id);

                HealthData.Remove(healthData);
            }
        }

        private bool HasData(NetworkId objectId) => HealthData.Any(x => x.ObjId == objectId);

        public void ApplyDamage(ApplyDamageContext damageContext)
        {
            bool isDamageFromServer = damageContext.IsFromServer;
            
            if (isDamageFromServer)
            {
                Debug.LogError($"Damage from server need to be refactored");
                return;
            }
            
            NetworkObject victimObj = damageContext.VictimObj;
            TeamSide shooterTeam = damageContext.Shooter.TeamSide;
            int damage = damageContext.Damage;
            SessionPlayer shooter = damageContext.Shooter;  
            bool isOwnerBot = damageContext.Shooter.IsBot;

            bool isDamagable = victimObj.TryGetComponent<IDamageable>(out var damagable);

            if (isDamagable == false)
            {
                Debug.Log($"Object {victimObj.name} is not damagable, skipping {damage} damage", victimObj);
                return;
            }

            NetworkId victimId = victimObj.Id;

            bool hasHealthDataForObject = HealthData.Any(x => x.ObjId == victimId);

            if (hasHealthDataForObject == false)
            {
                Debug.Log(
                    $" No Health Data presented for object {victimObj.name}, skipping {damage} damage. Probably missed initialization",
                    victimObj);
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
                    TeamSide victimTeamSide;

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

            if (isOwnerBot == false)
            {
                RPC_SpawnDamageHintFor(shooter.Owner, victimObj.transform.position, damage);
            }

            if (isDummyTarget)
            {
                DummyTarget dummyTarget = damagable as DummyTarget;

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

                string nickname = _playersDataService.GetNickname(victim);

                Debug.Log($"Player {nickname} got hit for {damage}");

                if (currentHealth == 0)
                {
                    OnPlayerHealthZero(playerCharacter.Object.StateAuthority, shooter);
                }
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

            ObjectWithHealthData data = new ObjectWithHealthData(victimId, (UInt16)currentHealth, healthData.MaxHealth);

            HealthData.Set(index, data);

            HealthChanged.OnNext(data);

            Debug.Log($"Damage {damage} applied to {victimObj.name}", victimObj);

            if (currentHealth <= 0)
            {
                HealthZero.OnNext(data);
            }

            return currentHealth;
        }

        private void OnBotHealthZero(Bot bot, SessionPlayer killer)
        {
            bot.RPC_OnDeath(true);
            
            var botDieContext = new UnitDieContext();
            botDieContext.Killer = killer;
            botDieContext.Victim = _sessionStateService.GetSessionPlayer(bot);

            bot.Alive = false;
            bot.View.transform.DOScale(0, 0.5f);

            BotDied.OnNext(botDieContext);

            //LoggerUI.Instance.Log($"Player {playerRef} is dead");

            float respawnTime = 2;

            Observable.Timer(TimeSpan.FromSeconds(respawnTime)).Subscribe((l =>
            {
                _botsController.RespawnBot(bot);
            }));
        }

        public void RestoreHealth(NetworkObject networkObject, bool isPlayer = false, bool isBot = false)
        {
            if (isPlayer)
            {   
                PlayerRef playerRef = networkObject.InputAuthority;

                CharacterClass playerCharacterClass = _playersDataService.GetPlayerCharacterClass(playerRef);
                CharacterData characterData = _gameStaticDataContainer.CharactersDataContainer.GetCharacterDataByClass(playerCharacterClass);

                GainHealthToPlayer(playerRef, characterData.CharacterStats.Health);
            }

            NetworkId id = networkObject.Id;

            if (HasData(id))
            {
                if (isBot)
                {
                    Bot bot = networkObject.GetComponent<Bot>();
                    CharacterClass characterClass = bot.BotData.CharacterClass;
                    CharacterData characterData = _gameStaticDataContainer.CharactersDataContainer.GetCharacterDataByClass(characterClass);
                    GainHealthTo(id, characterData.CharacterStats.Health);
                }
                else
                {
                    GainHealthTo(id, 100);
                }
            }
            
        }

        public void RestorePlayerHealth(PlayerRef playerRef)
        {
            RestoreHealth(_playersDataService.GetPlayerBase(playerRef).Object, true);
        }

        public void GainHealthToPlayer(PlayerRef playerRef, int health)
        {
            NetworkId playerId = _playersDataService.GetPlayer(playerRef).Object.Id;

            GainHealthTo(playerId, health);

            Debug.Log($"Gained {health} HP for player {playerRef}");
        }

        private void GainHealthTo(NetworkId id, int health)
        {   
            ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == id);
            int index = HealthData.IndexOf(healthData);
            ushort maxHealth = healthData.MaxHealth;
            
            int currentHealth = healthData.Health;
            currentHealth += health;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            ObjectWithHealthData data = new ObjectWithHealthData(id, (UInt16)currentHealth, maxHealth);
            HealthData.Set(index, data);    
        }

        private void OnPlayerHealthZero(PlayerRef victim, SessionPlayer killer)
        {
            PlayerCharacter playerCharacter = _playersDataService.GetPlayer(victim);
            PlayerBase playerBase = _playersDataService.GetPlayerBase(victim);

            playerCharacter.RPC_OnDeath();

            playerBase.PlayerController.SetAllowToMove(false);
            playerBase.PlayerController.SetAllowToShoot(false);

            var playerDieEventContext = new UnitDieContext();
            playerDieEventContext.Killer = killer;
            playerDieEventContext.Victim = _sessionStateService.GetSessionPlayer(victim);

            PlayerDied.OnNext(playerDieEventContext);       

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

    public struct ApplyDamageContext // TODO refactor
    {
        public int Damage;
        public SessionPlayer Shooter;
        public NetworkObject VictimObj;
        public bool IsFromServer;
    }
}