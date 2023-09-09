using Dev.Infrastructure;
using Dev.Weapons.Guns;
using Fusion;

namespace Dev.Levels
{
    public class Obstacle : NetworkContext, IObstacleDamageable
    {
        public PlayerRef PlayerRef => PlayerRef.None;
    }
}