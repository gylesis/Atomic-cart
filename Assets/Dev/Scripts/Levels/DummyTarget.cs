using Dev.Infrastructure;
using Dev.Weapons.Guns;
using Fusion;

namespace Dev.Levels
{
    public class DummyTarget : NetworkContext, IDamageable
    {
        public PlayerRef PlayerRef => PlayerRef.None;
    }
}