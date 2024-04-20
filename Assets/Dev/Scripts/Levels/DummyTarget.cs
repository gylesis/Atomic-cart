using Dev.Infrastructure;
using Dev.Utils;
using Dev.Weapons.Guns;

namespace Dev.Levels
{
    public class DummyTarget : NetworkContext, IDamageable
    {
        public int DamageId => AtomicConstants.DamageIds.ObstacleDamageId;
    }
}