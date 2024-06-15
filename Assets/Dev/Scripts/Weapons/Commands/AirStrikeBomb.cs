using Dev.Effects;
using Dev.Weapons.Guns;

namespace Dev.Weapons
{
    public class AirStrikeBomb : ExplosiveProjectile
    {
        public void StartDetonate()
        {
            ExplodeAndHitPlayers(_explosionRadius);  
            
            FxController.Instance.SpawnEffectAt("landmine_explosion", transform.position);

            ToDestroy.OnNext(this);
        }
        
    }
}