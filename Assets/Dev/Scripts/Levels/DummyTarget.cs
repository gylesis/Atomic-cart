using Dev.Infrastructure;
using Dev.Weapons.Guns;

namespace Dev.Levels
{
    public class DummyTarget : NetworkContext, IDamageable
    {
        public int Id => -2;
    }
}