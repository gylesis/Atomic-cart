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
        private ProcessCollisionContext _processCollisionContext;

        public Subject<HitContext> Hit { get; } = new();
        
        [Inject]
        private void Construct(HealthObjectsService healthObjectsService, TeamsService teamsService)
        {
            _teamsService = teamsService;
            _healthObjectsService = healthObjectsService;
        }

        public void ProcessCollision(ProcessCollisionContext context)
        {
            _processCollisionContext = context;
            
            Vector3 pos = context.OverlapPos;
            NetworkRunner runner = context.NetworkRunner;
            float overlapRadius = context.Radius;
            LayerMask hitMask = context.HitMask;
            int damage = context.Damage;
            bool isOwnerBot = context.IsOwnerBot;
            PlayerRef owner = runner.LocalPlayer;
            Projectile projectile = context.Projectile;

            var overlapSphere = Extensions.OverlapSphere(runner, pos, overlapRadius, hitMask, out var colliders);

            if (overlapSphere)
            {
                bool needToDestroy = false;

                foreach (Collider2D collider in colliders)
                {
                    var isDamageable = collider.TryGetComponent<IDamageable>(out var damagable);

                    if (isDamageable)
                    {
                        var isPlayer = collider.TryGetComponent<PlayerCharacter>(out var player);
    
                        if (isPlayer)
                        {
                            PlayerRef target = player.Object.StateAuthority;

                            if (target == owner) continue;

                            OnHit(player.Object,  owner, damage, HitType.Player, projectile);
                            needToDestroy = true;

                            break;
                        }
                        
                        if (damagable is IObstacleDamageable obstacleDamageable)
                        {
                            bool isStaticObstacle = damagable.DamageId == AtomicConstants.DamageIds.ObstacleDamageId;

                            if (isStaticObstacle)
                            {
                                OnHit((damagable as Obstacle).Object,  owner, damage, HitType.Obstacle, projectile);
                            }

                            bool isObstacleWithHealth = damagable.DamageId == AtomicConstants.DamageIds.ObstacleWithHealthDamageId;

                            if (isObstacleWithHealth)
                            {
                                OnHit((damagable as ObstacleWithHealth).Object,  owner, damage, HitType.ObstacleWithHealth, projectile);
                            }

                            needToDestroy = true;
                            break;
                        }

                        bool isDummyTarget = damagable.DamageId == AtomicConstants.DamageIds.DummyTargetDamageId;

                        if (isDummyTarget)
                        {
                            DummyTarget dummyTarget = damagable as DummyTarget;

                            OnHit(dummyTarget.Object,  owner, damage, HitType.Bot, projectile);
                            needToDestroy = true;

                            break;
                        }

                        bool isBot = damagable.DamageId == AtomicConstants.DamageIds.BotDamageId;

                        if (isBot)
                        {
                            if (isOwnerBot)
                            {
                                continue;  // TODO implement damage to other enemies bots
                            }
                            else
                            {
                                Bot targetBot = damagable as Bot;

                                TeamSide shooterTeam = _teamsService.GetUnitTeamSide(owner);
                                TeamSide botTeam = _teamsService.GetUnitTeamSide(targetBot);

                                if (shooterTeam != botTeam)
                                {
                                    OnHit(targetBot.Object,  owner, damage, HitType.Bot, projectile);
                                }
                            }
                        }
                        
                        needToDestroy = true;
                        break;
                    }
                }

                if (needToDestroy)
                {
                    projectile.ToDestroy.OnNext(projectile);
                }
            }
        }
        
        private void OnHit(NetworkObject networkObject, PlayerRef shooter, int damage, HitType hitType,
                           Projectile projectile)
        {
            if (hitType == HitType.Player || hitType == HitType.Bot)  
            {
                if (projectile is ExplosiveProjectile == false)
                {
                    _healthObjectsService.ApplyDamage(networkObject, shooter, damage);
                }
            }
            else if (hitType == HitType.ObstacleWithHealth)
            {
                ObstaclesManager.Instance.ApplyDamageToObstacle(shooter, networkObject.GetComponent<ObstacleWithHealth>(), damage);
            }

            //Debug.Log($"Damage for {hitType}");
            
            HitContext hitContext = new HitContext();   
            hitContext.GameObject = networkObject.gameObject;
            hitContext.HitType = hitType;
            
            Hit.OnNext(hitContext);
        }
     
    }
}   