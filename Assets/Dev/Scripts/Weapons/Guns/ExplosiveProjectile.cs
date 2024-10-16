﻿using Dev.BotsLogic;
using Dev.Levels;
using Dev.PlayerLogic;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public abstract class ExplosiveProjectile : Projectile
    {
        protected float _explosionRadius;

        public void Init(Vector3 moveDirection, float force, int damage, float explosionRadius)
        {
            SetExplosionRadius(explosionRadius);    

            Init(moveDirection, force, damage);
        }

        protected override void LoadLateInjection()
        {
            base.LoadLateInjection();
            _hitsProcessor.Explode.Subscribe(OnExplode).AddTo(this);
        }

        protected virtual void OnExplode(HitContext context)
        {
            RPC_PlaySound("explosion", transform.position, 40);
        }
        
        public void SetExplosionRadius(float explosionRadius)
        {
            _explosionRadius = explosionRadius;
        }
        
        protected override void OnPlayerHit(PlayerCharacter playerCharacter)
        {
            ExplodeAndDealDamage(_explosionRadius);
        }

        protected override void OnBotHit(Bot bot)
        {
            ExplodeAndDealDamage(_explosionRadius);
        }

        protected override void OnDummyHit(DummyTarget dummyTarget)
        {
            ExplodeAndDealDamage(_explosionRadius);
        }

        protected override void OnObstacleHit(Obstacle obstacle)
        {
            ExplodeAndDealDamage(_explosionRadius);
        }

        protected void ExplodeAndDealDamage(float explosionRadius)
        {   
            Vector3 pos = transform.position;   
            
            ProcessExplodeContext explodeContext = new ProcessExplodeContext(Owner, explosionRadius, Damage, pos, false);
            
            _hitsProcessor.ProcessExplodeAndHitUnits(explodeContext);
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            var position = transform.position;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, _explosionRadius);
            Handles.Label(position + Vector3.up * _explosionRadius + Vector3.right, "Explosion radius");
        }
#endif
        
    }
}