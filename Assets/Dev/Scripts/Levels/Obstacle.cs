using Dev.Infrastructure;
using Dev.Utils;

namespace Dev.Levels
{
    public class Obstacle : NetworkContext, IObstacleDamageable
    {
   
        public virtual int DamageId => AtomicConstants.DamageIds.ObstacleDamageId;
    }
}