using Dev.Effects;
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

    }
}