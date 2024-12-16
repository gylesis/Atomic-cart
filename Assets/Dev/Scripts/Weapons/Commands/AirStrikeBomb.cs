using Dev.Effects;
using Dev.Infrastructure;
using Dev.Weapons.Guns;

namespace Dev.Weapons.Commands
{
    public class AirStrikeBomb : ExplosiveProjectile
    {
        protected override bool CheckForHitsWhileMoving => false;
        
        public void Detonate()
        {
            ExplodeAndDealDamage(_explosionRadius);  
            
            FxController.Instance.SpawnEffectAt("landmine_explosion", transform.position);

            ToDestroy.OnNext(this);
        }


        protected override void OnExplodeShake()
        {
            CameraService.Instance.ShakeIfNeed("big_explosion", transform.position, Owner.IsBot);
        }
    }
}