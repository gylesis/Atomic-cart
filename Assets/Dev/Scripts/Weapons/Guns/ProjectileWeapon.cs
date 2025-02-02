﻿using System.Collections.Generic;
using System.Linq;
using Dev.Effects;
using Dev.Infrastructure;
using Dev.Sounds;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Dev.Weapons.StaticData;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public abstract class ProjectileWeapon<TProjectileType> : Weapon where TProjectileType : ProjectileStaticData
    {
        [SerializeField] private float _soundRadius = 40;

        public TProjectileType Data { get; protected set; }
        
        public float ProjectileSpeed => Data.ProjectileSpeed;

        public Projectile ProjectilePrefab => Data.ProjectilePrefab;

        protected List<SpawnedProjectileContext> _aliveProjectiles = new List<SpawnedProjectileContext>();

        protected override void Awake()
        {
            base.Awake();
            
            Data = GameSettingsProvider.GameSettings.WeaponStaticDataContainer.GetData<TProjectileType>();
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            PollAliveProjectilesForDestroy();
        }

        /// <summary>
        /// Polling all Projectiles for Max Distance, Alive Time
        /// </summary>
        private void PollAliveProjectilesForDestroy()
        {
            for (var index = _aliveProjectiles.Count - 1; index >= 0; index--)
            {
                SpawnedProjectileContext projectileContext = _aliveProjectiles[index];

                Projectile projectile = projectileContext.Projectile;

                Vector3 origin = projectileContext.Origin;

                //float distanceFromOrigin = (projectile.transform.position - origin).sqrMagnitude;

                TickTimer destroyTimer = projectile.DestroyTimer;

                var expired = destroyTimer.ExpiredOrNotRunning(Runner);

                if (expired)
                {
                    OnProjectileExpired(projectile);
                }
                
                
                /*else if (distanceFromOrigin > projectileContext.MaxDistance * projectileContext.MaxDistance)
                {
                    OnProjectileMaxDistanceReached(projectile);
                }*/
            }
        }

        /// <summary>
        /// Projectile initialization before spawn
        /// </summary>
        /// <param name="projectile"></param>
        protected virtual void OnProjectileBeforeSpawned(Projectile projectile) // auto destroy logic
        {
            DiInjecter.Instance.InjectGameObject(projectile.gameObject);
            
            projectile.RPC_SetOwner(Owner);
            
            projectile.ToDestroy.Take(1).TakeUntilDestroy(projectile).Subscribe((OnProjectileDestroy));
            projectile.DestroyTimer = TickTimer.CreateFromSeconds(Runner, 5);

            CameraService.Instance.ShakeIfNeed(Data.ShakePatternKey, transform.position, Owner.IsBot, false);

            Effect effect = FxController.Instance.SpawnEffectAt(Data.ShellsKey, _shootPoint.position);
            effect.transform.RotateTowardsDirection(Vector3.right);
            
            RPC_PlaySound(Data.FireSound, projectile.transform.position, 40);
            
            var projectileContext = new SpawnedProjectileContext();
            projectileContext.Projectile = projectile;
            projectileContext.Origin = projectile.transform.position;
            projectileContext.MaxDistance = Extensions.AtomicCart.GetBulletMaxDistanceClampedByWalls(ShootPos,
                ShootDirection, BulletMaxDistance, projectile.OverlapRadius);

            _aliveProjectiles.Add(projectileContext);
        }


        /// <summary>
        /// Event on Projectile destroy
        /// </summary>
        /// <param name="projectile"></param>
        private void OnProjectileDestroy(Projectile projectile)
        {
            projectile.SetViewStateLocal(false);
            //projectile.RPC_SetViewState(false);
            SpawnVFXOnDestroyProjectile(projectile);
            projectile.IsAlive = false;
            //RPC_LOG(Runner.LatestServerTick.Raw);

            DestroyProjectile(projectile, 0.5f);
        }

        /// <summary>
        /// Event on Projectile expired
        /// </summary>
        /// <param name="projectile"></param>
        protected virtual void OnProjectileExpired(Projectile projectile)
        {
            SpawnVFXOnDestroyProjectile(projectile);
            DestroyProjectile(projectile);
        }

        [Rpc]
        private void RPC_LOG(int tick)
        {
            //LoggerUI.Instance.Log($"Destroying on tick {tick}, current tick {Runner.LatestServerTick.Raw}");
        }

        /// <summary>
        /// Event on Projectile reached max distance
        /// </summary>
        /// <param name="projectile"></param>
        protected virtual void OnProjectileMaxDistanceReached(Projectile projectile)
        {
            DestroyProjectile(projectile);
        }

        /// <summary>
        /// Called when Max Distance reached, Alive timer expired or hit to obtacle or player
        /// </summary>
        /// <param name="projectile"></param>
        private void DestroyProjectile(Projectile projectile, float delay = 0)
        {
            SpawnSoundOnDestroyProjectile(projectile);
            
            DOVirtual.DelayedCall(delay, () =>
            {
                var exists = _aliveProjectiles.Exists(x => x.Projectile == projectile);
    
                if (exists)
                {
                    SpawnedProjectileContext context = _aliveProjectiles.First(x => x.Projectile == projectile);

                    _aliveProjectiles.Remove(context);
                
                    Runner.Despawn(projectile.Object);
                }
            } );
            
        }

        /// <summary>
        /// Managing which effect is going to spawn after destroy Projectile
        /// </summary>
        /// <param name="projectile"></param>
        protected virtual void SpawnVFXOnDestroyProjectile(Projectile projectile)
        {
            FxController.Instance.SpawnEffectAt("bullet_explosion", projectile.transform.position);
        }

        protected virtual void SpawnSoundOnDestroyProjectile(Projectile projectile)
        {
            RPC_PlaySound(Data.HitSound, projectile.transform.position, _soundRadius);
        }
        
        [Rpc(Channel = RpcChannel.Reliable)]
        private void RPC_PlaySound(NetworkString<_16> soundType, Vector3 at, float radius)
        {
            SoundController.Instance.PlaySoundAt(soundType.ToString(), at, radius);
        }
    }

    public struct SpawnedProjectileContext
    {
        public Projectile Projectile;
        public Vector3 Origin;
        public float MaxDistance;
    }
}