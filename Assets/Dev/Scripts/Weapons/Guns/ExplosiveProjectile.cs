using Dev.BotsLogic;
using Dev.Levels;
using Dev.PlayerLogic;
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

            Init(moveDirection, force, damage, explosionRadius);
        }

        public void SetExplosionRadius(float explosionRadius)
        {
            _explosionRadius = explosionRadius;
        }
        
        protected override void OnPlayerHit(PlayerCharacter playerCharacter)
        {
            base.OnPlayerHit(playerCharacter);
            ExplodeAndDealDamage(_explosionRadius);
        }

        protected override void OnBotHit(Bot bot)
        {
            base.OnBotHit(bot);
            ExplodeAndDealDamage(_explosionRadius);
        }

        protected override void OnDummyHit(DummyTarget dummyTarget)
        {
            base.OnDummyHit(dummyTarget);
            ExplodeAndDealDamage(_explosionRadius);
        }

        protected override void OnObstacleHit(Obstacle obstacle)
        {
            base.OnObstacleHit(obstacle);
            ExplodeAndDealDamage(_explosionRadius);
        }

        protected void ExplodeAndDealDamage(float explosionRadius)
        {   
            Vector3 pos = transform.position;   

            ProcessExplodeContext explodeContext = new ProcessExplodeContext(Owner, explosionRadius, Damage, pos, false);
            
            _hitsProcessor.ProcessExplodeAndHitUnits(explodeContext);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            var position = transform.position;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, _explosionRadius);
            Handles.Label(position + Vector3.up * _explosionRadius + Vector3.right, "Explosion radius");
        }
#endif
        
    }
}