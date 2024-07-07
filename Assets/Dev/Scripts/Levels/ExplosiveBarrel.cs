using Dev.Effects;
using Dev.Infrastructure;
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

            FxController.Instance.SpawnEffectAt<Effect>("barrel_explosion", transform.position);

            ExplodeAtAndHitPlayers(transform.position);
        }

        private void ExplodeAtAndHitPlayers(Vector3 pos) // TODO refactor
        {
            ProcessExplodeContext explodeContext = new ProcessExplodeContext(Runner,new SessionPlayer(), _explosionRadius, _damage, pos, _hitMask, true);
        
            _hitsProcessor.ProcessExplodeAndHitUnits(explodeContext);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}