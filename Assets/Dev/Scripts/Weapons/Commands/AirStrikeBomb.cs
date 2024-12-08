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
            
            FxController.Instance.SpawnEffectAt<Effect>("landmine_explosion", transform.position);

            ToDestroy.OnNext(this);
        }


        protected override void OnExplodeShake()
        {
            CameraService.Instance.ShakeIfNeed(transform.position, "big_explosion", Owner.IsBot);
        }
    }
}