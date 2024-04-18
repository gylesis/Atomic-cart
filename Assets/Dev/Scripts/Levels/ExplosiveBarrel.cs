using System;
using Dev.Effects;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UnityEngine;

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

            FxController.Instance.SpawnEffectAt("barrel_explosion", transform.position);

            ExplodeAtAndHitPlayers(transform.position);
        }

        private void ExplodeAtAndHitPlayers(Vector3 pos)
        {
            var overlapSphere = Extensions.OverlapSphere(Runner, pos, _explosionRadius, _hitMask, out var hits);

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
            
            PlayersHealthService.Instance.ApplyDamageFromServer(target, _damage);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}