using Dev.Infrastructure;
using Dev.Weapons.Guns;

namespace Dev.Levels
{
    public class Obstacle : NetworkContext, IObstacleDamageable
    {
        public virtual DamagableType DamageId => DamagableType.Obstacle;
    }
}