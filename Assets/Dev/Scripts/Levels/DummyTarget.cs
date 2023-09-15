using Dev.Infrastructure;
using Dev.Weapons.Guns;
using Fusion;

namespace Dev
{
    public class DummyTarget : NetworkContext, IDamageable
    {
        public int Id => PlayerRef.None;
    }
}