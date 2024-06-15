using Dev.BotsLogic;
using Dev.Levels;
using Dev.PlayerLogic;
using Fusion;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public abstract class ExplosiveProjectile : Projectile
    {
        protected float _explosionRadius;

        public void Init(Vector3 moveDirection, float force, int damage, PlayerRef owner, float explosionRadius)
        {
            _explosionRadius = explosionRadius;

            Init(moveDirection, force, damage, owner);
        }

        protected override void OnPlayerHit(PlayerCharacter playerCharacter)
        {
            base.OnPlayerHit(playerCharacter);
            ExplodeAndHitPlayers(_explosionRadius);
        }

        protected override void OnBotHit(Bot bot)
        {
            base.OnBotHit(bot);
            ExplodeAndHitPlayers(_explosionRadius);
        }

        protected override void OnObstacleHit(Obstacle obstacle)
        {
            base.OnObstacleHit(obstacle);
            ExplodeAndHitPlayers(_explosionRadius);
        }

        protected void ExplodeAndHitPlayers(float explosionRadius)
        {
            Vector3 pos = transform.position;

            ProcessExplodeContext explodeContext = new ProcessExplodeContext(Runner, explosionRadius, Damage, pos, _hitMask, OwnerTeamSide, OnObstacleWithHealthHit, OnDummyHit, OnUnitHit);
            
            _hitsProcessor.ProcessExplodeAndHitUnits(explodeContext);

            void OnUnitHit(NetworkObject obj, PlayerRef shooter, int totalDamage)
            {
                ApplyDamage(obj, shooter, totalDamage);
            }

            void OnDummyHit(NetworkObject obj, PlayerRef shooter, int totalDamage)
            {
                ApplyDamage(obj, shooter, totalDamage);
            }

            void OnObstacleWithHealthHit(ObstacleWithHealth obj, PlayerRef shooter, int totalDamage)
            {
                ApplyDamageToObstacle(obj, shooter, totalDamage);
            }
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