using Dev.Infrastructure;
using Dev.Weapons.Guns;
using Fusion;

namespace Dev
{
    public class Obstacle : NetworkContext, IObstacleDamageable
    {
        public PlayerRef PlayerRef => PlayerRef.None;
    }
}