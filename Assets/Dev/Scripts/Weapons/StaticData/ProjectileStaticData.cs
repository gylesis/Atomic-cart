using Dev.Weapons.Guns;
using UnityEngine;

namespace Dev.Weapons.StaticData
{
    public abstract class ProjectileStaticData : WeaponStaticData
    {
        [SerializeField] protected float _projectileSpeed = 1500f;
        [SerializeField] protected Projectile _projectilePrefab;

        public float ProjectileSpeed => _projectileSpeed;
        public Projectile ProjectilePrefab => _projectilePrefab;
    }
}