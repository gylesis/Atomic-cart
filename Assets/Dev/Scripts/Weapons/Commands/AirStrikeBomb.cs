using Dev.Effects;
using Dev.Weapons.Guns;

namespace Dev.Weapons
{
    public class AirStrikeBomb : ExplosiveProjectile
    {
        protected override bool CheckForHitsWhileMoving => false;
        
        public void StartDetonate()
        {
            ExplodeAndDealDamage(_explosionRadius);  
            
            FxController.Instance.SpawnEffectAt<Effect>("landmine_explosion", transform.position);

            ToDestroy.OnNext(this);
        }

    }
}