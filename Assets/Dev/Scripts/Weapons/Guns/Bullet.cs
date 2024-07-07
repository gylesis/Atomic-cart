namespace Dev.Weapons.Guns
{
    public class Bullet : Projectile
    {
        protected override bool CheckForHitsWhileMoving => true;
    }
}