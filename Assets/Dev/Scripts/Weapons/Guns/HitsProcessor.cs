using System;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Weapons.Guns
{
    public class HitsProcessor : NetworkContext
    {
        private HealthObjectsService _healthObjectsService;
        private TeamsService _teamsService;

        public Subject<HitContext> Hit { get; } = new();

        [Inject]
        private void Construct(HealthObjectsService healthObjectsService, TeamsService teamsService)
        {   
            _teamsService = teamsService;
            _healthObjectsService = healthObjectsService;
        }

        public void ProcessHitCollision(ProcessHitCollisionContext context)
        {
            NetworkRunner runner = Runner;
            SessionPlayer shooter = context.Owner;
            bool isHitFromServer = context.IsHitFromServer;
            TeamSide ownerTeamSide = context.OwnerTeamSide;
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
                    TeamSide targetTeamSide = targetPlayer.TeamSide;

                    //Debug.Log($"Hit to player {targetPlayerRef} from team {targetTeamSide}, by {owner} from team {ownerTeamSide}");

                    if (ownerTeamSide == targetTeamSide) continue;

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

                    TeamSide botTeam = targetBot.BotTeamSide;

                    if (ownerTeamSide != botTeam)
                    {
                        hitObject = targetBot.Object;
                        break;
                    }
                }
            }

            bool isProjectileHitSomething = hitObject != null;
            
            if (isProjectileHitSomething)
            {
                if (projectile.Collided == false)
                {
                    projectile.Collided = true;
                    OnHit(hitObject, shooter, damage, damagableType, projectile is ExplosiveProjectile, isHitFromServer);
                    Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe((l =>
                    {
                        projectile.ToDestroy.OnNext(projectile);
                    }));
                }
                else
                {
                    projectile.SetViewStateLocal(false);
                }

               
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

        public void ProcessExplodeAndHitUnits(ProcessExplodeContext explodeContext)
        {
            RPC_ProcessExplodeInternal(explodeContext);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ProcessExplodeInternal(ProcessExplodeContext context)
        {
            NetworkRunner runner = Runner;
            SessionPlayer owner = context.Owner;
            bool isDamageFromServer = context.IsDamageFromServer;
            TeamSide ownerTeamSide = context.OwnerTeamSide;

            Vector3 pos = context.ExplosionPos;
            float explosionRadius = context.ExplosionRadius;
            int damage = context.Damage;

            var overlapSphere = Extensions.OverlapCircle(runner, pos, explosionRadius, out var colliders);

            if (overlapSphere == false) return;
            
            float maxDistance = (pos - (pos + Vector3.right * explosionRadius)).sqrMagnitude;

            foreach (Collider2D collider in colliders)
            {
                bool isDamagable = collider.TryGetComponent<IDamageable>(out var damagable);

                if (isDamagable == false) continue;

                bool isDummyTarget = damagable.DamageId == DamagableType.DummyTarget;
                bool isBot = damagable.DamageId == DamagableType.Bot;
                bool isStaticObstacle = damagable.DamageId == DamagableType.Obstacle;
                bool isObstacleWithHealth = damagable.DamageId == DamagableType.ObstacleWithHealth;
                bool isPlayer = damagable.DamageId == DamagableType.Player;

                float distance = (collider.transform.position - pos).sqrMagnitude;
                float damagePower = 1 - distance / maxDistance;
                damagePower = 1;

                //Debug.Log($"DMG power {damagePower}");

                int totalDamage = (int)(damagePower * damage);

                if (isStaticObstacle) { }

                if (isObstacleWithHealth)
                {
                    ObstacleWithHealth obstacle = (damagable as ObstacleWithHealth);

                    OnExplode(obstacle.Object, owner, DamagableType.ObstacleWithHealth, totalDamage,
                        isDamageFromServer);

                    continue;
                }

                if (isDummyTarget)
                {
                    DummyTarget dummyTarget = damagable as DummyTarget;

                    OnExplode(dummyTarget.Object, owner, DamagableType.DummyTarget, totalDamage,
                        isDamageFromServer);

                    continue;
                }

                if (isPlayer)
                {
                    PlayerCharacter playerCharacter = damagable as PlayerCharacter;

                    PlayerRef target = playerCharacter.Object.StateAuthority;

                    TeamSide targetTeamSide = _teamsService.GetUnitTeamSide(target);

                    //Debug.Log($"owner teamside {ownerTeamSide}, target team {targetTeamSide}");

                    if (ownerTeamSide == targetTeamSide) continue;

                    OnExplode(playerCharacter.Object, owner, DamagableType.Player, totalDamage,
                        isDamageFromServer);

                    continue;
                }

                if (isBot)
                {
                    // TODO implement damage to other enemies bots

                    Bot targetBot = damagable as Bot;

                    TeamSide targetTeamSide = _teamsService.GetUnitTeamSide(targetBot);

                    if (ownerTeamSide == targetTeamSide) continue;

                    OnExplode(targetBot.Object, owner, DamagableType.Bot, totalDamage, isDamageFromServer);

                    continue;
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
                
                Debug.Log($"Damage from explosion");
                _healthObjectsService.ApplyDamage(damageContext);
            }
        }

    }
}