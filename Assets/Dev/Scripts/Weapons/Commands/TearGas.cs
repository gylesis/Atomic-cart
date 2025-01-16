using Dev.Weapons.Guns;
using UnityEngine;

namespace Dev.Weapons.Commands
{
    public class TearGas : ExplosiveProjectile
    {
        protected override bool CheckForHitsWhileMoving => false;
        
        public void DealDamage()
        {
            ExplodeAndDealDamage(_explosionRadius);  
        }

        protected override void OnExplode(HitContext context)
        {
            base.OnExplode(context);
            //RPC_PlaySound("teargas", transform.position, 40);
        }
       
    }
}