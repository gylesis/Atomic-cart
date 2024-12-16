using System;
using Dev.Effects;
using Dev.Infrastructure;
using Dev.Utils;
using Dev.Weapons.StaticData;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class GrenadeLauncher : ProjectileWeapon<GrenadeLauncherStaticData>
    {
        private float GrenadeExplosionRadius => Data.GrenadeExplosionRadius;
        private float GrenadeFlyTime => Data.GrenadeFlyTime;
        private float DetonateTime => Data.GrenadeDetonateTime;
        
        
        public override void Shoot(Vector2 direction, float power = 1)
        {
            Projectile projectile = Runner.Spawn(ProjectilePrefab, ShootPos, Quaternion.identity,
                Object.InputAuthority, (runner, o) =>
                {
                    GrenadeProjectile projectile = o.GetComponent<GrenadeProjectile>();

                    BulletHitOverlapRadius = projectile.OverlapRadius;
                    
                    float maxDistance = Extensions.AtomicCart.GetBulletMaxDistanceClampedByWalls(transform.position, ShootDirection, BulletMaxDistance, projectile.OverlapRadius);
                    
                    Vector2 targetPos = (Vector2) transform.position + (direction * maxDistance);

                    projectile.Init(direction, ProjectileSpeed, Damage, GrenadeExplosionRadius,
                          targetPos, GrenadeFlyTime, DetonateTime);

                    OnProjectileBeforeSpawned(projectile);
                }); 
        }

        protected override void SpawnVFXOnDestroyProjectile(Projectile projectile)
        {
            FxController.Instance.SpawnEffectAt("bazooka_projectile_explosion", projectile.transform.position);
        }
    }
}