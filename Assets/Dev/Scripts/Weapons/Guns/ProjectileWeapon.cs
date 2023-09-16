using System.Collections.Generic;
using System.Linq;
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
    
        [SerializeField] protected Projectile _projectilePrefab;

        protected List<SpawnedProjectileContext> _aliveProjectiles = new List<SpawnedProjectileContext>();

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority == false) return;

            PollAliveProjectilesForDestroy();
        }

        private void PollAliveProjectilesForDestroy()
        {
            for (var index = _aliveProjectiles.Count - 1; index >= 0; index--)
            {
                SpawnedProjectileContext projectileContext = _aliveProjectiles[index];
                
                Projectile projectile = projectileContext.Projectile;

                Vector3 origin = projectileContext.Origin;

                float distanceFromOrigin = (projectile.transform.position - origin).sqrMagnitude;

                TickTimer destroyTimer = projectile.DestroyTimer;

                var expired = destroyTimer.ExpiredOrNotRunning(Runner);
                
                if (distanceFromOrigin > _bulletMaxDistance * _bulletMaxDistance)
                {
                    OnProjectileMaxDistanceReached(projectile);
                }
                else if(expired)
                {
                    OnProjectileExpired(projectile);
                }
                
            }
        }

        protected virtual void OnProjectileBeforeSpawned(Projectile projectile)
        {
            projectile.ToDestroy.Take(1).TakeUntilDestroy(projectile).Subscribe((OnProjectileDestroy));
            projectile.DestroyTimer = TickTimer.CreateFromSeconds(Runner, 10);

            var projectileContext = new SpawnedProjectileContext();
            projectileContext.Projectile = projectile;
            projectileContext.Origin = projectile.transform.position;

            _aliveProjectiles.Add(projectileContext);
        }

        private void OnProjectileDestroy(Projectile projectile)
        {
            DestroyProjectile(projectile);
        }

        protected virtual void OnProjectileExpired(Projectile projectile)
        {
            Debug.Log($"Projectile {projectile} expired, destroying");
            DestroyProjectile(projectile);
        }

        protected virtual void OnProjectileMaxDistanceReached(Projectile projectile)
        {
            DestroyProjectile(projectile);
        }
        
        private void DestroyProjectile(Projectile projectile)
        {
            var exists = _aliveProjectiles.Exists(x => x.Projectile == projectile);

            if (exists)
            {
                SpawnedProjectileContext context = _aliveProjectiles.First(x => x.Projectile == projectile);

                _aliveProjectiles.Remove(context);
                SpawnVFXOnDestroyProjectile(projectile);
                Runner.Despawn(projectile.Object);
            }
        }

        protected virtual void SpawnVFXOnDestroyProjectile(Projectile projectile)
        {
            FxController.Instance.SpawnEffectAt("bullet_explosion", projectile.transform.position);
        }
    }

    public struct SpawnedProjectileContext
    {
        public Projectile Projectile;
        public Vector3 Origin;
    }
    
}


