namespace Dev.Weapons.Guns
{
    public class BazookaProjectile : ExplosiveProjectile
    {
        protected override bool CheckForHitsWhileMoving => true;
    }
}