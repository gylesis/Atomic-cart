using Dev.BotsLogic;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public abstract class ExplosiveProjectile : Projectile
    {
        protected float _explosionRadius;

        public void Init(Vector3 moveDirection, float force, int damage, PlayerRef owner, float explosionRadius)
        {
            _explosionRadius = explosionRadius;

            Init(moveDirection, force, damage, owner);
        }

        protected override void OnPlayerHit(PlayerCharacter playerCharacter)
        {
            base.OnPlayerHit(playerCharacter);
            ExplodeAndHitPlayers(_explosionRadius);
        }

        protected override void OnBotHit(Bot bot)
        {
            base.OnBotHit(bot);
            ExplodeAndHitPlayers(_explosionRadius);
        }

        protected override void OnObstacleHit(Obstacle obstacle)
        {
            base.OnObstacleHit(obstacle);
            ExplodeAndHitPlayers(_explosionRadius);
        }

        protected void ExplodeAndHitPlayers(float explosionRadius)
        {
            Vector3 pos = transform.position;

            var overlapSphere = Extensions.OverlapSphere(Runner, pos, explosionRadius, _hitMask, out var hits);

            if (overlapSphere)
            {
                float maxDistance = (pos - (pos + Vector3.right * explosionRadius)).sqrMagnitude;

                PlayerRef shooter = Object.StateAuthority;

                
                foreach (Collider2D collider in hits)
                {
                    var isDamageable = collider.TryGetComponent<IDamageable>(out var damagable);

                    if (isDamageable)
                    {
                        float distance = (collider.transform.position - pos).sqrMagnitude;

                        float damagePower = 1 - distance / maxDistance;

                        damagePower = 1;

                        //Debug.Log($"DMG power {damagePower}");

                        int totalDamage = (int)(damagePower * _damage);
                        
                        if (damagable is IObstacleDamageable obstacleDamageable)
                        {
                            bool isStaticObstacle = damagable.DamageId == AtomicConstants.DamageIds.ObstacleDamageId;

                            if (isStaticObstacle) { }

                            bool isObstacleWithHealth =
                                damagable.DamageId == AtomicConstants.DamageIds.ObstacleWithHealthDamageId;

                            if (isObstacleWithHealth)
                            {
                                ApplyDamageToObstacle(damagable as ObstacleWithHealth, shooter, totalDamage);
                            }

                            continue;
                        }

                        bool isDummyTarget = damagable.DamageId == -2;

                        if (isDummyTarget)
                        {
                            DummyTarget dummyTarget = damagable as DummyTarget;

                            ApplyDamage(dummyTarget.Object, shooter, totalDamage);

                            continue;
                        }

                        var isPlayer = collider.TryGetComponent<PlayerCharacter>(out var player);

                        if (isPlayer)
                        {
                            PlayerRef target = player.Object.StateAuthority;

                            if (target == shooter) continue;

                            ApplyDamage(player.Object, shooter, totalDamage);
                            ApplyForceToPlayer(player, Vector2.right, damagePower * 50);
                            
                            continue;
                        }
                        
                        bool isBot = damagable.DamageId == AtomicConstants.DamageIds.BotDamageId;

                        if (isBot)
                        {
                            // if (isOwnerBot)
                            //  {
                            //     continue;  // TODO implement damage to other enemies bots
                            //  }
                            //  else
                            //   {

                            Bot targetBot = damagable as Bot;

                            ApplyDamage(targetBot.Object, shooter, (int)(damagePower * _damage));
                            
                            continue;
                        }
                    }
                }
            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}