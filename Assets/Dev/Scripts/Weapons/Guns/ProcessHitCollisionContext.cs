using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public struct ProcessHitCollisionContext
    {
        public NetworkRunner NetworkRunner { get; private set; }

        public Vector3 OverlapPos { get; private set; }

        public float Radius { get; private set; }

        public int Damage { get; private set; }

        public LayerMask HitMask { get; private set; }
        
        public bool IsOwnerBot { get; private set; }
        
        public Projectile Projectile { get; private set; }
       
        public TeamSide OwnerTeamSide { get; private set; }
        
        public SessionPlayer Owner { get; private set; }
        
        public bool IsHitFromServer { get; private set; }
        
        
        public ProcessHitCollisionContext(NetworkRunner networkRunner, Projectile projectile, Vector3 overlapPos, float radius, int damage, LayerMask hitMask, bool isOwnerBot, bool isHitFromServer, SessionPlayer owner, TeamSide ownerTeamSide)
        {
            IsHitFromServer = isHitFromServer;
            Owner = owner;          
            NetworkRunner = networkRunner;
            OverlapPos = overlapPos;
            Radius = radius;
            Damage = damage;
            HitMask = hitMask;
            IsOwnerBot = isOwnerBot;
            OwnerTeamSide = ownerTeamSide;
            Projectile = projectile;
        }
    }
    
}