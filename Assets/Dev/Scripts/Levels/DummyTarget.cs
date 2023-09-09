using Dev.Infrastructure;
using Dev.Weapons.Guns;
using Fusion;

namespace Dev.Levels
{
    public class DummyTarget : NetworkContext, IDamageable
    {
        public int Id { get; } = -1;
    }
}