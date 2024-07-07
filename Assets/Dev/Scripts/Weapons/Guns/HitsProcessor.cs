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
        private ProcessHitCollisionContext _processHitCollisionContext;
        private SessionStateService _sessionStateService;

        public Subject<HitContext> Hit { get; } = new();

        [Inject]
        private void Construct(HealthObjectsService healthObjectsService, TeamsService teamsService, SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
            _teamsService = teamsService;
            _healthObjectsService = healthObjectsService;
        }

        public void ProcessHitCollision(ProcessHitCollisionContext context)
        {
            _processHitCollisionContext = context;

            Vector3 pos = context.OverlapPos;
            NetworkRunner runner = context.NetworkRunner;
            float overlapRadius = context.Radius;
            LayerMask hitMask = context.HitMask;
            int damage = context.Damage;
            bool isOwnerBot = context.IsOwnerBot;
            PlayerRef owner = runner.LocalPlayer;
            Projectile projectile = context.Projectile;
            TeamSide ownerTeamSide = context.OwnerTeamSide;

            var overlapSphere = Extensions.OverlapCircle(runner, pos, overlapRadius, hitMask, out var colliders);

            if (overlapSphere)
            {
                bool needToDestroy = false;

                foreach (Collider2D collider in colliders)
                {
                    bool isDamagable = collider.TryGetComponent<IDamageable>(out var damagable);

                    if (isDamagable == false) continue;

                    bool isDummyTarget = damagable.DamageId == DamagableType.DummyTarget;
                    bool isBot = damagable.DamageId == DamagableType.Bot;
                    bool isStaticObstacle = damagable.DamageId == DamagableType.Obstacle;
                    bool isObstacleWithHealth = damagable.DamageId == DamagableType.ObstacleWithHealth;
                    bool isPlayer = damagable.DamageId == DamagableType.Player;

                    if (isPlayer)
                    {
                        PlayerCharacter targetPlayer = damagable as PlayerCharacter;
                        PlayerRef targetPlayerRef = targetPlayer.Object.InputAuthority;
                        TeamSide targetTeamSide = _teamsService.GetUnitTeamSide(targetPlayerRef);

                        //Debug.Log($"Hit to player {targetPlayerRef} from team {targetTeamSide}, by {owner} from team {ownerTeamSide}");

                        if (ownerTeamSide == targetTeamSide) continue;

                        OnHit(targetPlayer.Object, owner, damage, DamagableType.Player, projectile);
                        needToDestroy = true;

                        break;
                    }

                    if (isObstacleWithHealth)
                    {
                        NetworkObject networkObject = collider.GetComponent<NetworkObject>();

                        OnHit(networkObject, owner, damage, DamagableType.ObstacleWithHealth,
                            projectile);

                        needToDestroy = true;
                        break;
                    }

                    if (isStaticObstacle)
                    {
                        NetworkObject networkObject = collider.GetComponent<NetworkObject>();

                        OnHit(networkObject, owner, damage, DamagableType.Obstacle, projectile);

                        needToDestroy = true;
                        break;
                    }

                    if (isDummyTarget)
                    {
                        DummyTarget dummyTarget = damagable as DummyTarget;

                        OnHit(dummyTarget.Object, owner, damage, DamagableType.Bot, projectile);
                        needToDestroy = true;

                        break;
                    }

                    if (isBot)
                    {
                        Bot targetBot = damagable as Bot;

                        TeamSide botTeam = targetBot.BotTeamSide;

                        if (ownerTeamSide != botTeam)
                        {
                            Debug.Log($"Damage to bot");
                            OnHit(targetBot.Object, owner, damage, DamagableType.Bot, projectile);
                            needToDestroy = true;
                            break;
                        }
                    }
                }

                if (needToDestroy)
                {
                    projectile.ToDestroy.OnNext(projectile);
                }
            }
        }


        private void OnHit(NetworkObject networkObject, PlayerRef shooter, int damage, DamagableType damagableType,
                           Projectile projectile)
        {
            switch (damagableType)
            {
                case DamagableType.ObstacleWithHealth:
                    ObstaclesManager.Instance.ApplyDamageToObstacle(shooter,
                        networkObject.GetComponent<ObstacleWithHealth>(), damage);
                    break;
                case DamagableType.Obstacle:
                    break;
                case DamagableType.Bot:
                case DamagableType.Player:
                    if (projectile is ExplosiveProjectile == false)
                    {
                        ApplyDamageContext damageContext = new ApplyDamageContext();
                        damageContext.Damage = damage;
                        damageContext.VictimObj = networkObject;
                        damageContext.Shooter = _processHitCollisionContext.Owner;

                        _healthObjectsService.ApplyDamage(damageContext);
                    }

                    break;
                case DamagableType.DummyTarget:
                    break;
                default:
                    break;
            }

            //Debug.Log($"Damage type: {damagableType},");

            HitContext hitContext = new HitContext();
            hitContext.GameObject = networkObject.gameObject;
            hitContext.DamagableType = damagableType;

            Hit.OnNext(hitContext);
        }

        public void ProcessExplodeAndHitUnits(ProcessExplodeContext explodeContext)
        {
            NetworkRunner runner = explodeContext.NetworkRunner;
            Vector3 pos = explodeContext.ExplosionPos;
            float explosionRadius = explodeContext.ExplosionRadius;
            TeamSide ownerTeamSide = explodeContext.OwnerTeamSide;
            LayerMask hitMask = explodeContext.HitMask;
            int damage = explodeContext.Damage;
            PlayerRef owner = explodeContext.Owner;

            var overlapSphere = Extensions.OverlapCircle(runner, pos, explosionRadius, hitMask, out var colliders);

            if (overlapSphere)
            {
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

                    if (isDamagable)
                    {
                        float distance = (collider.transform.position - pos).sqrMagnitude;

                        float damagePower = 1 - distance / maxDistance;

                        damagePower = 1;

                        //Debug.Log($"DMG power {damagePower}");

                        int totalDamage = (int)(damagePower * damage);

                        if (isStaticObstacle) { }

                        if (isObstacleWithHealth)
                        {
                            explodeContext.ObstacleWithHealthHit?.Invoke(damagable as ObstacleWithHealth, owner, totalDamage);

                            continue;
                        }

                        if (isDummyTarget)
                        {
                            DummyTarget dummyTarget = damagable as DummyTarget;

                            explodeContext.DummyHit?.Invoke(dummyTarget.Object, owner, totalDamage);

                            continue;
                        }

                        if (isPlayer)
                        {
                            PlayerCharacter playerCharacter = damagable as PlayerCharacter;

                            PlayerRef target = playerCharacter.Object.StateAuthority;

                            TeamSide targetTeamSide = _teamsService.GetUnitTeamSide(target);

                            Debug.Log($"owner teamside {ownerTeamSide}, target team {targetTeamSide}");
                            
                            if (ownerTeamSide == targetTeamSide) continue;

                            explodeContext.UnitHit?.Invoke(playerCharacter.Object, owner, totalDamage);

                            continue;
                        }

                        if (isBot)
                        {
                            // TODO implement damage to other enemies bots
                            
                            Bot targetBot = damagable as Bot;

                            TeamSide targetTeamSide = _teamsService.GetUnitTeamSide(targetBot);

                            if (ownerTeamSide == targetTeamSide) continue;
                            
                            explodeContext.UnitHit?.Invoke(targetBot.Object, owner, totalDamage);

                            continue;
                        }
                    }
                }
            }
        }
    }
}