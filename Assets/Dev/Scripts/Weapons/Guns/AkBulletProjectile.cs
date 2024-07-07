namespace Dev.Weapons.Guns
{
    public class AkBulletProjectile : Projectile
    {
        protected override bool CheckForHitsWhileMoving => true;
    }
}