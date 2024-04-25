using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public struct ProcessCollisionContext
    {
        public NetworkRunner NetworkRunner { get; private set; }

        public Vector3 OverlapPos { get; private set; }

        public float Radius { get; private set; }

        public int Damage { get; private set; }

        public LayerMask HitMask { get; private set; }
        
        public bool IsOwnerBot { get; private set; }
        
        public Projectile Projectile { get; private set; }
    
        public ProcessCollisionContext(NetworkRunner networkRunner, Projectile projectile, Vector3 overlapPos, float radius, int damage, LayerMask hitMask, bool isOwnerBot)
        {
            NetworkRunner = networkRunner;
            OverlapPos = overlapPos;
            Radius = radius;
            Damage = damage;
            HitMask = hitMask;
            IsOwnerBot = isOwnerBot;
            Projectile = projectile;
        }
    }
}