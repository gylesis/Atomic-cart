namespace Dev.Weapons.Guns
{
    public class PistolBulletProjectile : Projectile
    {
        protected override bool CheckForHitsWhileMoving => true;
    }
}