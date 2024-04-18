using System;
using Dev.Effects;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Weapons.StaticData;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class BazookaWeapon : ProjectileWeapon<BazookaStaticData>
    {
        public float ExplosionRadius => GameSettingProvider.GameSettings.WeaponStaticDataContainer.GetData<BazookaStaticData>().ExplosionRadius;
        public float FirePushPower =>  GameSettingProvider.GameSettings.WeaponStaticDataContainer.GetData<BazookaStaticData>().FirePushPower;

        public override void Shoot(Vector2 direction, float power = 1)
        {
            PushForcePlayer(direction);

            Projectile projectile = Runner.Spawn(ProjectilePrefab, ShootPos, Quaternion.identity,
                Object.InputAuthority, (runner, o) =>
                {
                    BazookaProjectile projectile = o.GetComponent<BazookaProjectile>();

                    BulletHitOverlapRadius = projectile.OverlapRadius;
                    
                    projectile.Init(direction, ProjectileSpeed, Damage, Object.InputAuthority, ExplosionRadius);

                    OnProjectileBeforeSpawned(projectile);
                });
        }

        private void PushForcePlayer(Vector2 direction) // TODO temp, need better way to apply this
        {
            NetworkObject networkObject = Runner.GetPlayerObject(Object.InputAuthority);
            PlayerCharacter playerCharacter = networkObject.GetComponent<PlayerCharacter>();

            playerCharacter.Rigidbody.velocity = -direction * FirePushPower;
            // player.Rigidbody.AddForce(-direction * _firePushPower, ForceMode2D.Impulse);
            playerCharacter.PlayerController.SetAllowToMove(false);
            Observable.Timer(TimeSpan.FromSeconds(0.5f))
                .Subscribe((l => { playerCharacter.PlayerController.SetAllowToMove(true); }));
        }

        protected override void SpawnVFXOnDestroyProjectile(Projectile projectile)
        {
            FxController.Instance.SpawnEffectAt("bazooka_projectile_explosion", projectile.transform.position);
        }
    }
}