using Dev.Effects;
using Dev.Infrastructure;
using Dev.Weapons.Guns;
using UnityEngine;

namespace Dev.Levels
{
    public class ExplosiveBarrel : ObstacleWithHealth
    {
        [SerializeField] private float _explosionRadius;

        [SerializeField] private int _damage = 30;

        [ContextMenu("Explode")]
        private void Explode()
        {
            OnZeroHealth();
        }
        
        public override void OnZeroHealth()
        {
            base.OnZeroHealth();

            FxController.Instance.SpawnEffectAt<Effect>("barrel_explosion", transform.position);
            CameraService.Instance.ShakeIfNeed(transform.position, "small_explosion", false);

            ExplodeAtAndHitPlayers(transform.position);
        }

        private void ExplodeAtAndHitPlayers(Vector3 pos) // TODO refactor
        {
            ProcessExplodeContext explodeContext = new ProcessExplodeContext(new SessionPlayer(), _explosionRadius, _damage, pos,  true);
        
            _hitsProcessor.ProcessExplodeAndHitUnits(explodeContext);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }

}