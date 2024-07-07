using System;
using Dev.Effects;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.Levels
{
    public class ExplosiveBarrel : ObstacleWithHealth
    {
        [SerializeField] private float _explosionRadius;
        [SerializeField] private LayerMask _hitMask;

        [SerializeField] private int _damage = 30;
        
        
        public override void OnZeroHealth()
        {
            base.OnZeroHealth();

            FxController.Instance.SpawnEffectAt<Effect>("barrel_explosion", transform.position);

            ExplodeAtAndHitPlayers(transform.position);
        }

        private void ExplodeAtAndHitPlayers(Vector3 pos) // TODO refactor
        {
            return;
            ProcessExplodeContext explodeContext = new ProcessExplodeContext();
            explodeContext.Damage = _damage;
            explodeContext.NetworkRunner = Runner;
            explodeContext.Owner = PlayerRef.None;
            
            _hitsProcessor.ProcessExplodeAndHitUnits(explodeContext);
            
            var overlapSphere = Extensions.OverlapCircle(Runner, pos, _explosionRadius, _hitMask, out var hits);

            if (overlapSphere)
            {
                foreach (Collider2D collider in hits)
                {
                    var isPlayer = collider.gameObject.TryGetComponent<PlayerCharacter>(out var player);

                    if (isPlayer)
                    {
                        OnPlayerHit(player);
                    }
                }
            }
        }

        private void OnPlayerHit(PlayerCharacter playerCharacter)
        {
            PlayerRef target = playerCharacter.Object.InputAuthority;

            ApplyDamageContext damageContext = new ApplyDamageContext();
            damageContext.Damage = _damage;
            damageContext.IsFromServer = true;
            damageContext.VictimObj = playerCharacter.Object;
            
            _healthObjectsService.ApplyDamage(damageContext);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}