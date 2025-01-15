using Dev.Effects;
using Dev.Infrastructure;
using Dev.Sounds;
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

            DangerZoneViewProvider.Instance.SetDangerZoneView(transform.position, _explosionRadius, 1.5f);
            FxController.Instance.SpawnEffectAt("barrel_explosion", transform.position);
            CameraService.Instance.ShakeIfNeed("small_explosion", transform.position, false);
            SoundController.Instance.PlaySoundAt("explosion", transform.position, 40);

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