using Dev.Levels;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class BazookaProjectile : Projectile
    {
        private float _explosionRadius;

        public void Init(Vector3 moveDirection, float force, int damage, PlayerRef owner, float explosionRadius)
        {
            _explosionRadius = explosionRadius;

            Init(moveDirection, force, damage, owner);
        }

        protected override void ApplyHitToPlayer(Player player)
        {
            ExplodeAtAndHitPlayers(transform.position);
        }

        protected override void OnObstacleHit(Obstacle obstacle)
        {
            ExplodeAtAndHitPlayers(transform.position);
        }

        private void ExplodeAtAndHitPlayers(Vector3 pos)
        {
            var overlapSphere = OverlapSphere(pos, _explosionRadius, _hitMask, out var hits);

            if (overlapSphere)
            {
                float maxDistance = (pos - (pos + Vector3.right * _explosionRadius)).sqrMagnitude;

                foreach (LagCompensatedHit hit in hits)
                {
                    var isPlayer = hit.GameObject.TryGetComponent<Player>(out var player);

                    if (isPlayer)
                    {
                        PlayerRef owner = Object.InputAuthority;
                        PlayerRef target = player.Object.InputAuthority;

                        if (target == owner) continue;

                        float distance = (hit.GameObject.transform.position - pos).sqrMagnitude;

                        float damagePower = 1 - distance / maxDistance;

                        damagePower = 1;

                        Debug.Log($"DMG power {damagePower}");

                        ApplyDamage(player, owner, (int)(damagePower * _damage));

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