using System.Collections.Generic;
using Dev.Effects;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    [OrderAfter(typeof(Projectile))]
    public abstract class ProjectileWeapon : Weapon
    {
        [SerializeField] protected float _projectileSpeed = 15f;
        [SerializeField] protected float _projectileAliveTime = 1;

        [SerializeField] protected Projectile _projectilePrefab;

        protected List<Projectile> _aliveProjectiles = new List<Projectile>();

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority == false) return;

            PollAliveProjectilesForDestroy();
        }

        private void PollAliveProjectilesForDestroy()
        {
            for (var index = _aliveProjectiles.Count - 1; index >= 0; index--)
            {
                Projectile projectile = _aliveProjectiles[index];

                TickTimer destroyTimer = projectile.DestroyTimer;

                var expired = destroyTimer.ExpiredOrNotRunning(Runner);

                if (expired)
                {
                    OnProjectileExpired(projectile);
                }
            }
        }

        protected virtual void OnProjectileBeforeSpawned(Projectile projectile)
        {
            projectile.ToDestroy.Take(1).TakeUntilDestroy(projectile).Subscribe((OnProjectileDestroy));
            projectile.DestroyTimer = TickTimer.CreateFromSeconds(Runner, _projectileAliveTime);
            
            _aliveProjectiles.Add(projectile);
        }

        private void OnProjectileDestroy(Projectile projectile)
        {
            DestroyProjectile(projectile);
        }

        protected virtual void OnProjectileExpired(Projectile projectile)
        {
            DestroyProjectile(projectile);
        }

        private void DestroyProjectile(Projectile projectile)
        {
            _aliveProjectiles.Remove(projectile);
            FxController.Instance.SpawnEffectAt("bullet_explosion", projectile.transform.position);
            Runner.Despawn(projectile.Object);
        }
    }
}