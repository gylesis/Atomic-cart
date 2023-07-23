using System.Collections.Generic;
using System.Linq;
using Dev.Effects;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public abstract class ProjectileWeapon : Weapon
    {
        [SerializeField] protected float _projectileSpeed = 15f;
        [SerializeField] protected float _projectileAliveTime = 1;
        
        [SerializeField] protected Projectile _projectilePrefab;

        protected Dictionary<Projectile, TickTimer> _aliveBullets = new Dictionary<Projectile, TickTimer>();


        private List<Projectile> _toRemove = new List<Projectile>(10);

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            
            if (Object.HasStateAuthority == false) return;

            
            foreach (var pair in _aliveBullets)
            {
                TickTimer destroyTimer = pair.Value;
                Projectile projectile = pair.Key;

                var expired = destroyTimer.Expired(Runner);

                if (expired)
                {
                    OnProjectileExpired(projectile);

                    _toRemove.Add(projectile);
                }
            }
            
            for (var index = _toRemove.Count - 1; index     >= 0; index    --)
            {
                Projectile projectile = _toRemove[index];
                
                Runner.Despawn(projectile.Object);

                _aliveBullets.Remove(projectile);
                _toRemove.Remove(projectile);
            }
        }

        protected virtual void OnProjectileExpired(Projectile projectile)
        {
            FxController.Instance.SpawnEffectAt("bullet_explosion", projectile.transform.position);
        }
        
        protected virtual void OnProjectileBeforeSpawned(Projectile projectile)
        {
            _aliveBullets.Add(projectile, TickTimer.CreateFromSeconds(Runner, _projectileAliveTime));
        }
    }
}