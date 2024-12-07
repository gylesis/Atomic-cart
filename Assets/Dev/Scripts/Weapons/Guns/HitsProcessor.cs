using System;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Sounds;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Weapons.Guns
{
    public class HitsProcessor : NetworkContext
    {
        [Networked, Capacity(4)] private NetworkLinkedList<HitRecord> HitRecords { get; }
        
        private HealthObjectsService _healthObjectsService;
        private PlayersDataService _playersDataService;
        private SessionStateService _sessionStateService;
        private GameSettings _gameSettings;

        public Subject<HitContext> Hit { get; } = new();
        public Subject<HitContext> Explode { get; } = new();

        [Inject]
        private void Construct(HealthObjectsService healthObjectsService, SessionStateService sessionStateService, PlayersDataService playersDataService, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _sessionStateService = sessionStateService;
            _playersDataService = playersDataService;
            _healthObjectsService = healthObjectsService;   
        }

        /// <summary>
        /// Used for processing projectile hit logic
        /// </summary>
        /// <param name="context"></param>
        public void ProcessHit(ProcessHitCollisionContext context)
        {
            NetworkRunner runner = Runner;
            SessionPlayer shooter = context.Owner;
            bool isDamageFromServer = context.IsHitFromServer;
            TeamSide ownerTeamSide;
            {
                var hasTeam = _sessionStateService.TryGetPlayerTeam(shooter, out ownerTeamSide);

                if (hasTeam == false)
                {
                    AtomicLogger.Err(hasTeam.ErrorMessage);
                    return;
                }
            }

            bool isOwnerBot = context.IsOwnerBot;

            Vector3 pos = context.OverlapPos;
            float overlapRadius = context.Radius;
            int damage = context.Damage;

            var overlapSphere = Extensions.OverlapCircle(runner, pos, overlapRadius, out var colliders);

            if (overlapSphere == false) return;

            Projectile projectile = Runner.FindObject(context.ProjectileId).GetComponent<Projectile>();
            
            NetworkObject hitObject = null;
            DamagableType damagableType = DamagableType.DummyTarget;

            foreach (Collider2D cldr in colliders)
            {
                bool isDamagable = cldr.TryGetComponent<IDamageable>(out var damagable);

                if (isDamagable == false) continue;

                bool isDummyTarget = damagable.DamageId == DamagableType.DummyTarget;
                bool isBot = damagable.DamageId == DamagableType.Bot;
                bool isStaticObstacle = damagable.DamageId == DamagableType.Obstacle;
                bool isObstacleWithHealth = damagable.DamageId == DamagableType.ObstacleWithHealth;
                bool isPlayer = damagable.DamageId == DamagableType.Player;

                damagableType = damagable.DamageId;

                if (isPlayer)
                {
                    PlayerCharacter targetPlayer = damagable as PlayerCharacter;

                    //Debug.Log($"Hit to player {targetPlayerRef} from team {targetTeamSide}, by {owner} from team {ownerTeamSide}");

                    if (isDamageFromServer == false)
                    {
                        var hasTeam = _sessionStateService.TryGetPlayerTeam(targetPlayer, out var targetTeamSide);

                        if (hasTeam == false)
                        {
                            AtomicLogger.Err(hasTeam.ErrorMessage);
                            return;
                        }
                        
                        if (ownerTeamSide == targetTeamSide) continue;
                    }

                    hitObject = targetPlayer.Object;
                    break;
                }

                if (isObstacleWithHealth)
                {
                    NetworkObject networkObject = cldr.GetComponent<NetworkObject>();
                    hitObject = networkObject;
                    break;
                }

                if (isStaticObstacle)
                {
                    NetworkObject networkObject = cldr.GetComponent<NetworkObject>();

                    hitObject = networkObject;
                    break;
                }

                if (isDummyTarget)
                {
                    DummyTarget dummyTarget = damagable as DummyTarget;
                    hitObject = dummyTarget.Object;
                    break;
                }

                if (isBot)
                {
                    Bot targetBot = damagable as Bot;

                    if (isDamageFromServer == false)
                    {
                        var hasTeam = _sessionStateService.TryGetPlayerTeam(targetBot.BotData.SessionPlayer, out var botTeam);
                        
                        if (hasTeam == false)
                        {
                            AtomicLogger.Err(hasTeam.ErrorMessage);
                            return;
                        }
                        
                        if (ownerTeamSide == botTeam) continue;
                    }
                    
                    hitObject = targetBot.Object;
                    break;
                }
            }

            bool isProjectileHitSomething = hitObject != null;

            if (isProjectileHitSomething)
            {
                OnHit(hitObject, shooter, damage, damagableType, projectile is ExplosiveProjectile, isDamageFromServer);
                
                projectile.ToDestroy.OnNext(projectile);
            }
        }

        private void OnHit(NetworkObject networkObject, SessionPlayer shooter, int damage, DamagableType damagableType,
                           bool isExplosionProjectile, bool isHitFromServer)
        {
            if (damagableType != DamagableType.Obstacle && isExplosionProjectile == false)
            {
                ApplyDamageContext damageContext = new ApplyDamageContext();
                damageContext.IsFromServer = isHitFromServer;
                damageContext.Damage = damage;
                damageContext.VictimObj = networkObject;
                damageContext.Shooter = shooter;

                //Debug.Log($"Damage from hit");
                _healthObjectsService.ApplyDamage(damageContext);
            }

            //Debug.Log($"Damage type: {damagableType},");

            HitContext hitContext = new HitContext();
            hitContext.GameObject = networkObject.gameObject;
            hitContext.DamagableType = damagableType;

            Hit.OnNext(hitContext);
        }

      
        /// <summary>
        /// Used for processing collision excluding collision check owner
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="originalObjectId"></param>
        /// <returns></returns>
        public bool ProcessCollision(NetworkRunner runner, Vector3 pos, float radius, PlayerRef originalObjectId)
        {
            var overlapSphere = Extensions.OverlapCircle(runner, pos, radius, out var colliders);

            if (overlapSphere == false) return false;
            
            foreach (Collider2D cldr in colliders)
            {
                bool isDamagable = cldr.TryGetComponent<IDamageable>(out var damagable);

                if (isDamagable)
                {
                    if (cldr.GetComponent<NetworkObject>().StateAuthority == originalObjectId)
                        return false;
                    
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Used for processing collision excluding collision check owner
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="originalObjectId"></param>
        /// <returns></returns>
        public bool ProcessCollision(NetworkRunner runner, Vector3 pos, float radius, NetworkId originalObjectId)
        {
            var overlapSphere = Extensions.OverlapCircle(runner, pos, radius, out var colliders);

            if (overlapSphere == false) return false;
            
            foreach (Collider2D cldr in colliders)
            {
                bool isDamagable = cldr.TryGetComponent<IDamageable>(out var damagable);

                if (isDamagable)
                {
                    if (cldr.GetComponent<NetworkObject>().Id == originalObjectId)
                        return false;
                    
                    return true;
                }
            }
            
            return false;
        }


        /// <summary>
        /// Used for processing explosion and hit logic at the same time
        /// </summary>
        /// <param name="explodeContext"></param>
        /// <param name="onExploded"></param>
        public void ProcessExplodeAndHitUnits(ProcessExplodeContext explodeContext, Action<NetworkObject, SessionPlayer, DamagableType, int, bool> onExploded = null)
        {   
            NetworkRunner runner = Runner;  
            SessionPlayer owner = explodeContext.Owner;
            bool isDamageFromServer = explodeContext.IsDamageFromServer;
            TeamSide ownerTeamSide;
            {
                var hasTeam = _sessionStateService.TryGetPlayerTeam(owner, out ownerTeamSide);

                if (hasTeam == false)
                {
                    return;
                }
            }
            
            
            Vector3 pos = explodeContext.ExplosionPos;
            float explosionRadius = explodeContext.ExplosionRadius;
            int damage = explodeContext.Damage;

            var overlapSphere = Extensions.OverlapCircleExcludeWalls(runner, pos, explosionRadius, out var targets);

            if (overlapSphere == false) return;

            float maxDistance = (pos - (pos + Vector3.right * explosionRadius)).sqrMagnitude;

            foreach (Collider2D target in targets)  
            {
                bool isDamagable = target.TryGetComponent<IDamageable>(out var damagable);

                if (isDamagable == false) continue;

                bool isDummyTarget = damagable.DamageId == DamagableType.DummyTarget;
                bool isBot = damagable.DamageId == DamagableType.Bot;
                bool isStaticObstacle = damagable.DamageId == DamagableType.Obstacle;
                bool isObstacleWithHealth = damagable.DamageId == DamagableType.ObstacleWithHealth;
                bool isPlayer = damagable.DamageId == DamagableType.Player;

                float distance = (target.transform.position - pos).sqrMagnitude;
                float damagePower = 1 - distance / maxDistance;
                damagePower = 1;

                DamagableType damagableType = DamagableType.DummyTarget;
                NetworkObject victim = null;
                
                //Debug.Log($"DMG power {damagePower}");

                int totalDamage = (int)(damagePower * damage);

                if (isStaticObstacle) { }

                if (isObstacleWithHealth)
                {
                    ObstacleWithHealth obstacle = (damagable as ObstacleWithHealth);

                    victim = obstacle.Object;
                    damagableType = DamagableType.ObstacleWithHealth;
                }

                if (isDummyTarget)
                {
                    DummyTarget dummyTarget = damagable as DummyTarget;

                    victim = dummyTarget.Object;
                    damagableType = DamagableType.DummyTarget;
                }

                if (isPlayer)
                {
                    PlayerCharacter playerCharacter = damagable as PlayerCharacter;

                    //Debug.Log($"owner teamside {ownerTeamSide}, target team {targetTeamSide}");

                    if (isDamageFromServer == false)
                    {
                        PlayerRef targetPlayer = playerCharacter.Object.StateAuthority;
                        var hasTeam = _sessionStateService.TryGetPlayerTeam(targetPlayer, out TeamSide targetTeamSide);
                        
                        if (hasTeam.IsError && ownerTeamSide == targetTeamSide) continue;
                    }

                    victim = playerCharacter.Object;
                    damagableType = DamagableType.Player;
                }

                if (isBot)
                {
                    // TODO implement damage to other enemies bots

                    Bot targetBot = damagable as Bot;

                    if (isDamageFromServer == false)
                    {
                        var hasTeam = _sessionStateService.TryGetPlayerTeam(targetBot.BotData.SessionPlayer, out TeamSide targetTeamSide);

                        if (hasTeam.IsError && ownerTeamSide == targetTeamSide) continue;
                    }

                    victim = targetBot.Object;
                    damagableType = DamagableType.Player;
                }

                if (victim != null)
                {
                    if (damage == -1)
                    {
                        Debug.Log($"Damage is -1, not applying damage, just returned callback");
                        
                        onExploded?.Invoke(victim, owner, damagableType, totalDamage, isDamageFromServer); // refactor
                    }
                    else
                    {
                        OnExplode(victim, owner, damagableType, totalDamage, isDamageFromServer);
                    }
                    
                }
            }
        }


        private void OnExplode(NetworkObject networkObject, SessionPlayer shooter, DamagableType damagableType,
                               int damage, bool isDamageFromServer)
        {
            if (damagableType != DamagableType.Obstacle)
            {
                ApplyDamageContext damageContext = new ApplyDamageContext();

                damageContext.IsFromServer = isDamageFromServer;
                damageContext.Damage = damage;
                damageContext.VictimObj = networkObject;
                damageContext.Shooter = shooter;

                //Debug.Log($"Damage from explosion");
                _healthObjectsService.ApplyDamage(damageContext);
            }
            
            HitContext hitContext = new HitContext();
            hitContext.GameObject = networkObject.gameObject;
            hitContext.DamagableType = damagableType;

            Explode.OnNext(hitContext);
        }
        
        
        // TODO save hit history
        /*public void OnHit(SessionPlayer owner, SessionPlayer victim, float damage, DateTime time)
        {
            HitRecord hitRecord = new HitRecord(owner, victim, damage, time.ToFileTime(), false);
            HitRecords.Add(hitRecord);
        }

        public void OnServerHit(SessionPlayer victim, float damage, DateTime time)
        {
            HitRecord hitRecord = new HitRecord(SessionPlayer.Default, victim, damage, time.ToFileTime(), true);
            HitRecords.Add(hitRecord);
        }*/
        
    }
}