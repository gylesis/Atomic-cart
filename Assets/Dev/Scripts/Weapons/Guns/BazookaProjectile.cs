using Fusion;

namespace Dev.Weapons.Guns
{
    
    public class BazookaProjectile : Projectile
    {
        
 

        protected override void ApplyHitToPlayer(Player player)
        {
            base.ApplyHitToPlayer(player);
            
        }

        protected override void OnObstacleHit(LagCompensatedHit obstacleHit)
        {
            var overlapSphere = OverlapSphere(transform.position, _overlapRadius, _hitMask, out var hits);
            
        }
    }
}