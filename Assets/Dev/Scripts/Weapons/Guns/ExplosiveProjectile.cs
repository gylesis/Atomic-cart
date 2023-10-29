using Dev.Levels;
using Dev.PlayerLogic;
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
        
        protected override void ApplyHitToPlayer(Player player)
        {
            ExplodeAndHitPlayers(_explosionRadius);
        }

        protected override void OnObstacleHit(Obstacle obstacle)
        {
            ExplodeAndHitPlayers(_explosionRadius);
        }
        
        protected void ExplodeAndHitPlayers(float explosionRadius)
        {
            Vector3 pos = transform.position;

            var overlapSphere = OverlapSphere(pos, explosionRadius, _hitMask, out var hits);

            if (overlapSphere)
            {
                float maxDistance = (pos - (pos + Vector3.right * explosionRadius)).sqrMagnitude;

                PlayerRef shooter = Object.InputAuthority;

                foreach (LagCompensatedHit hit in hits)
                {
                    var isDamageable = hit.GameObject.TryGetComponent<IDamageable>(out var damagable);

                    if (isDamageable == false || damagable is IObstacleDamageable obstacleDamageable)
                    {
                        bool isObstacleWithHealth = damagable.Id == 0;

                        if (isObstacleWithHealth)
                        {
                            ApplyDamageToObstacle(damagable as ObstacleWithHealth, shooter, _damage);
                        }

                        continue;
                    }
                    
                    bool isDummyTarget = damagable.Id == -2;

                    if (isDummyTarget)
                    {
                        DummyTarget dummyTarget = damagable as DummyTarget;

                        ApplyDamageToDummyTarget(dummyTarget, shooter, _damage);

                        continue;
                    }
                    
                    var isPlayer = hit.GameObject.TryGetComponent<Player>(out var player);

                    if (isPlayer)
                    {
                        PlayerRef target = player.Object.InputAuthority;

                        if (target == shooter) continue;

                        float distance = (hit.GameObject.transform.position - pos).sqrMagnitude;

                        float damagePower = 1 - distance / maxDistance;

                        damagePower = 1;

                        Debug.Log($"DMG power {damagePower}");

                        ApplyDamage(player, shooter, (int)(damagePower * _damage));

                        ApplyForceToPlayer(player, Vector2.right, damagePower * 50);
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