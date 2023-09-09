using System;
using Dev.Effects;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class BazookaWeapon : ProjectileWeapon
    {
        [SerializeField] private float _explosionRadius = 2;
        [SerializeField] private float _firePushPower = 5;

        public override void Shoot(Vector2 direction, float power = 1)
        {
            PushForcePlayer(direction);

            Projectile projectile = Runner.Spawn(_projectilePrefab, ShootPos, Quaternion.identity,
                Object.InputAuthority, (runner, o) =>
                {
                    BazookaProjectile projectile = o.GetComponent<BazookaProjectile>();

                    projectile.Init(direction, _projectileSpeed, Damage, Object.InputAuthority, _explosionRadius);

                    OnProjectileBeforeSpawned(projectile);
                });
        }

        private void PushForcePlayer(Vector2 direction) // TODO temp, need better way to apply this
        {
            NetworkObject networkObject = Runner.GetPlayerObject(Object.InputAuthority);
            Player player = networkObject.GetComponent<Player>();

            player.Rigidbody.velocity = -direction * _firePushPower;
            // player.Rigidbody.AddForce(-direction * _firePushPower, ForceMode2D.Impulse);
            player.PlayerController.AllowToMove = false;
            Observable.Timer(TimeSpan.FromSeconds(0.5f))
                .Subscribe((l => { player.PlayerController.AllowToMove = true; }));
        }

        protected override void SpawnVFXOnDestroyProjectile(Projectile projectile)
        {
            FxController.Instance.SpawnEffectAt("bazooka_projectile_explosion", projectile.transform.position);
        }
    }
}