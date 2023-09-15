using Dev.Infrastructure;

namespace Dev.Levels
{
    public class Obstacle : NetworkContext, IObstacleDamageable
    {
   
        public virtual int Id => -1;
    }
}