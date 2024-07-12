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

        public void RegisterPlayer(PlayerRef playerRef)
        {
            RPC_RegisterPlayerInternal(playerRef);
        }

        public int GetHealth(NetworkId id)
        {   
            if (HasData(id) == false)
            {
                Debug.Log($"No data for object with id {id}");
                return -1;
            }
    
            return HealthData.First(x => x.ObjId == id).Health;
        }

        public bool IsFullHealth(NetworkId id)
        {
            if (HasData(id) == false)
            {
                Debug.Log($"No data for object with id {id}");
                return false;
            }

            ObjectWithHealthData data = HealthData.First(x => x.ObjId == id);
            
            return data.Health == data.MaxHealth;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RegisterPlayerInternal(PlayerRef playerRef)
        {
            CharacterClass playerCharacterClass = _playersDataService.GetPlayerCharacterClass(playerRef);
            CharacterData characterData = _gameStaticDataContainer.CharactersDataContainer.GetCharacterDataByClass(playerCharacterClass);

            RPC_InternalRegisterObject(playerRef.ToNetworkId(), characterData.CharacterStats.Health);
        }
            
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
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
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_InternalRegisterObject(NetworkId networkId, int health)
        {   
            if (HasData(networkId))
            {
                Debug.Log($"Trying to register already registered object");
                return;
            }

            health = Mathf.Clamp(health, 0, UInt16.MaxValue); // to avoid uint overflow

            ObjectWithHealthData healthData = new ObjectWithHealthData(networkId, (UInt16)health, (UInt16)health);

            HealthData.Add(healthData);

            //Debug.Log($"Registering health object {networkObject.name}, total count {HealthData.Count}", networkObject);
        }
        

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_UnregisterObject(NetworkObject networkObject)
        {
            if (Runner == null) return;

            NetworkId id = networkObject.Id;
            
            if (HasData(id))
            {
                ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == id);

                HealthData.Remove(healthData);
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_UnregisterObject(NetworkId objId)
        {
            if (Runner == null) return;
            
            if (HasData(objId)) 
            {
                ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == objId);

                HealthData.Remove(healthData);
            }
        }
       

        private bool HasData(NetworkId objectId) => HealthData.Any(x => x.ObjId == objectId);
    
        public void ApplyDamage(ApplyDamageContext damageContext)   
        {
            bool isDamageFromServer = damageContext.IsFromServer;
            
            NetworkObject victimObj = damageContext.VictimObj;
            TeamSide shooterTeam = damageContext.Shooter.TeamSide;
            int damage = damageContext.Damage;
            SessionPlayer shooter = damageContext.Shooter;  
            bool isOwnerBot = damageContext.Shooter.IsBot;

            bool isTargetDamagable = victimObj.TryGetComponent<IDamageable>(out var damagable);

            if (isTargetDamagable == false)
            {
                Debug.Log($"Object {victimObj.name} is not damagable, skipping {damage} damage", victimObj);
                return;
            }
            // target
            bool isDummyTarget = damagable.DamageId == DamagableType.DummyTarget;
            bool isBot = damagable.DamageId == DamagableType.Bot;
            bool isStaticObstacle = damagable.DamageId == DamagableType.Obstacle;
            bool isObstacleWithHealth = damagable.DamageId == DamagableType.ObstacleWithHealth;
            bool isPlayer = damagable.DamageId == DamagableType.Player;

            
            Color hintDamageColor = GetHintDamageColor(damagable.DamageId);
            
            NetworkId victimId = GetVictimId(isPlayer, victimObj);

            if (HasData(victimId) == false)
            {
                Debug.Log("No Health Data presented for object {victimObj.name}, skipping {damage} damage. " +
                          "Probably missed initialization",
                    victimObj);
                return;
            }
            
            
            if (isDamageFromServer == false)
            {
                if (isPlayer || isBot) //
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

                        if (victimTeamSide == shooterTeam)
                        {
                            Debug.Log($"Damage from the same team, skipping damage");
                            return;
                        }
                    }
                }
            }

            if (isOwnerBot == false)
            {
                RPC_SpawnDamageHintFor(shooter.Owner, victimObj.transform.position, damage, hintDamageColor);
            }

            if (isDummyTarget)
            {
                DummyTarget dummyTarget = damagable as DummyTarget;

                Debug.Log($"Damage {damage} applied to dummy target {dummyTarget.name}");

                damage = 0;
            }

            int currentHealth = ApplyDamageInternal(victimId, damage);

            if (isPlayer)
            {
                PlayerCharacter playerCharacter = damagable as PlayerCharacter;
                PlayerRef victim = victimObj.StateAuthority;

                RPC_SpawnDamageHintFor(victim, victimObj.transform.position, damage, hintDamageColor);

                string nickname = _playersDataService.GetNickname(victim);

                //Debug.Log($"Player {nickname} got hit for {damage}");

                if (currentHealth == 0)
                {
                    OnPlayerHealthZero(playerCharacter.Object.StateAuthority, shooter, isDamageFromServer);
                }
            }

            if (isBot)
            {
                Bot bot = damagable as Bot;

                if (currentHealth == 0)
                {
                    OnBotHealthZero(bot, shooter, isDamageFromServer);
                }
            }

            if (isObstacleWithHealth)
            {
                ObstacleWithHealth obstacle = damagable as ObstacleWithHealth;

                if (currentHealth <= 0)
                {
                    OnObstacleZeroHealth(obstacle);
                }
            }
        }

        private static NetworkId GetVictimId(bool isPlayer, NetworkObject victimObj)
        {
            NetworkId victimId;

            if (isPlayer)
            {
                victimId = victimObj.StateAuthority.ToNetworkId();
            }
            else
            {
                victimId = victimObj.Id;
            }

            return victimId;
        }

        private Color GetHintDamageColor(DamagableType damagableType)
        {
            Color color;
            
            switch (damagableType)
            {
                case DamagableType.ObstacleWithHealth:
                    color = Color.cyan;
                    break;
                case DamagableType.Player:
                    color = Color.yellow;
                    break;
                case DamagableType.DummyTarget:
                    color = Color.gray;
                    break;
                default:
                    color = Color.red;
                    break;
            }

            return color;
        }
        
        private int ApplyDamageInternal(NetworkId victimId, int damage)
        {
            damage = Mathf.Clamp(damage, 0, UInt16.MaxValue); // to avoid uint overflow

            ObjectWithHealthData healthData = HealthData.First(x => x.ObjId == victimId);
            int index = HealthData.IndexOf(healthData);

            int currentHealth = healthData.Health;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, UInt16.MaxValue);

            ObjectWithHealthData data = new ObjectWithHealthData(victimId, (UInt16)currentHealth, healthData.MaxHealth);

            HealthData.Set(index, data);

            HealthChanged.OnNext(data);

            //Debug.Log($"Damage {damage} applied to {victimObj.name}", victimObj);

            if (currentHealth <= 0)
            {
                HealthZero.OnNext(data);
            }

            return currentHealth;
        }

        private void OnBotHealthZero(Bot bot, SessionPlayer killer, bool isDamageFromServer)
        {
            bot.RPC_OnDeath(true);
            
            var dieContext = new UnitDieContext();
            dieContext.Killer = killer;
            dieContext.Victim = _sessionStateService.GetSessionPlayer(bot);
            dieContext.IsKilledByServer = isDamageFromServer;
            
            bot.Alive = false;
            bot.View.RPC_Scale(0);

            BotDied.OnNext(dieContext);

            //LoggerUI.Instance.Log($"Player {playerRef} is dead");

            float respawnTime = 2;

            Observable.Timer(TimeSpan.FromSeconds(respawnTime)).Subscribe((l =>
            {
                _botsController.RPC_RespawnBot(bot);
            }));
        }
        
        private void OnPlayerHealthZero(PlayerRef victim, SessionPlayer killer, bool isKilledByServer)
        {
            PlayerCharacter playerCharacter = _playersDataService.GetPlayer(victim);
            PlayerBase playerBase = _playersDataService.GetPlayerBase(victim);

            playerCharacter.RPC_OnDeath();

            playerBase.PlayerController.SetAllowToMove(false);
            playerBase.PlayerController.SetAllowToShoot(false);

            var dieContext = new UnitDieContext();
            dieContext.Killer = killer;
            dieContext.Victim = _sessionStateService.GetSessionPlayer(victim);
            dieContext.IsKilledByServer = isKilledByServer;

            PlayerDied.OnNext(dieContext);       

            //LoggerUI.Instance.Log($"Player {playerRef} is dead");

            float respawnTime = 2;

            Observable.Timer(TimeSpan.FromSeconds(respawnTime)).Subscribe((l =>
            {
                RestoreHealth(playerCharacter.Object, true);
                _playersDataService.PlayersSpawner.RespawnPlayerCharacter(victim);
            }));
        }

        private void OnObstacleZeroHealth(ObstacleWithHealth obstacle)
        {
            obstacle.OnZeroHealth();

            Observable.Timer(TimeSpan.FromSeconds(_gameSettings.BarrelsRespawnCooldown)).Subscribe((l =>
            {
                RestoreObstacle(obstacle);
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

        public void RestoreObstacle(ObstacleWithHealth obstacle)
        {
            obstacle.Restore();
            RestoreHealth(obstacle.Object);
        }
        
        public void GainHealthToPlayer(PlayerRef playerRef, int health)
        {
            NetworkId playerId = playerRef.ToNetworkId();

            GainHealthTo(playerId, health);

            //Debug.Log($"Gained {health} HP for player {playerRef}");
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

        [Rpc]
        private void RPC_SpawnDamageHintFor([RpcTarget] PlayerRef playerRef, Vector3 pos, int damage, Color color)
        {
            _worldTextProvider.SpawnDamageText(pos, damage, color);
        }
        
    }

    public struct ApplyDamageContext // TODO refactor
    {
        public SessionPlayer Shooter;
        public NetworkObject VictimObj;
        public int Damage;
        public bool IsFromServer;
    }
}