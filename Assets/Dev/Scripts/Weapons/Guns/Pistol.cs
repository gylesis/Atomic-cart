using Dev.Weapons.StaticData;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class Pistol : ProjectileWeapon<PistolStaticData>
    {
        public override void Shoot(Vector2 direction, float power = 1)
        {
            Projectile projectile = Runner.Spawn(ProjectilePrefab, ShootPos, Quaternion.identity,
                Object.InputAuthority, (runner, o) =>
                {
                    Projectile projectile = o.GetComponent<Projectile>();

                    BulletHitOverlapRadius = projectile.OverlapRadius;
                    
                    projectile.Init(direction, ProjectileSpeed, Damage);

                    OnProjectileBeforeSpawned(projectile);
                });
        }
    }
}