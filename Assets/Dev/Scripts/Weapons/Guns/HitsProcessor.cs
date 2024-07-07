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
            Vector3 pos = context.OverlapPos;
            NetworkRunner runner = context.NetworkRunner;
            float overlapRadius = context.Radius;
            LayerMask hitMask = context.HitMask;
            int damage = context.Damage;
            bool isOwnerBot = context.IsOwnerBot;
            Projectile projectile = context.Projectile;
            TeamSide ownerTeamSide = context.OwnerTeamSide;
            bool isHitFromServer = context.IsHitFromServer;
            SessionPlayer shooter = context.Owner;

            var overlapSphere = Extensions.OverlapCircle(runner, pos, overlapRadius, hitMask, out var colliders);

            if (overlapSphere == false) return;
            
            bool needToDestroy = false;

            foreach (Collider2D cldr in colliders)
            {
                bool isDamagable = cldr.TryGetComponent<IDamageable>(out var damagable);

                if (isDamagable == false) continue;

                bool isDummyTarget = damagable.DamageId == DamagableType.DummyTarget;
                bool isBot = damagable.DamageId == DamagableType.Bot;
                bool isStaticObstacle = damagable.DamageId == DamagableType.Obstacle;
                bool isObstacleWithHealth = damagable.DamageId == DamagableType.ObstacleWithHealth;
                bool isPlayer = damagable.DamageId == DamagableType.Player;

                if (isPlayer)
                {
                    PlayerCharacter targetPlayer = damagable as PlayerCharacter;
                    TeamSide targetTeamSide = targetPlayer.TeamSide;

                    //Debug.Log($"Hit to player {targetPlayerRef} from team {targetTeamSide}, by {owner} from team {ownerTeamSide}");

                    if (ownerTeamSide == targetTeamSide) continue;

                    OnHit(targetPlayer.Object, shooter, damage, DamagableType.Player, projectile, isHitFromServer);
                    needToDestroy = true;

                    break;
                }

                if (isObstacleWithHealth)
                {
                    NetworkObject networkObject = cldr.GetComponent<NetworkObject>();

                    OnHit(networkObject, shooter, damage, DamagableType.ObstacleWithHealth,
                        projectile, isHitFromServer);

                    needToDestroy = true;
                    break;
                }

                if (isStaticObstacle)
                {
                    NetworkObject networkObject = cldr.GetComponent<NetworkObject>();

                    OnHit(networkObject, shooter, damage, DamagableType.Obstacle, projectile, isHitFromServer);

                    needToDestroy = true;
                    break;
                }

                if (isDummyTarget)
                {
                    DummyTarget dummyTarget = damagable as DummyTarget;

                    OnHit(dummyTarget.Object, shooter, damage, DamagableType.Bot, projectile, isHitFromServer);
                    needToDestroy = true;

                    break;
                }

                if (isBot)
                {
                    Bot targetBot = damagable as Bot;

                    TeamSide botTeam = targetBot.BotTeamSide;

                    if (ownerTeamSide != botTeam)
                    {
                        OnHit(targetBot.Object, shooter, damage, DamagableType.Bot, projectile, isHitFromServer);
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


        private void OnHit(NetworkObject networkObject, SessionPlayer shooter, int damage, DamagableType damagableType,
                           Projectile projectile, bool isHitFromServer)
        {
            if (damagableType != DamagableType.Obstacle && projectile is ExplosiveProjectile == false)
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
            NetworkRunner runner = explodeContext.NetworkRunner;
            Vector3 pos = explodeContext.ExplosionPos;
            float explosionRadius = explodeContext.ExplosionRadius;
            TeamSide ownerTeamSide = explodeContext.OwnerTeamSide;
            LayerMask hitMask = explodeContext.HitMask;
            int damage = explodeContext.Damage;
            SessionPlayer owner = explodeContext.Owner;
            bool isDamageFromServer = explodeContext.IsDamageFromServer;

            var overlapSphere = Extensions.OverlapCircle(runner, pos, explosionRadius, hitMask, out var colliders);

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