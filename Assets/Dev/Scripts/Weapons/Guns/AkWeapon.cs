using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class AkWeapon : ProjectileWeapon
    {
        public override void Shoot(Vector2 direction, float power = 1)
        {
            Projectile projectile = Runner.Spawn(_projectilePrefab, ShootPos, Quaternion.identity,
                Object.InputAuthority, (runner, o) =>
                {
                    Projectile projectile = o.GetComponent<Projectile>();

                    projectile.Init(direction, _projectileSpeed, Damage, Object.InputAuthority);

                    OnProjectileBeforeSpawned(projectile);
                });
        }
    }
}