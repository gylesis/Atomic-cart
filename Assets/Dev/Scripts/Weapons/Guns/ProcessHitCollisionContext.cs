using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public struct ProcessHitCollisionContext : INetworkStruct   
    {
        public SessionPlayer Owner { get; private set; }
        public bool IsHitFromServer { get; private set; }
        public bool IsOwnerBot => Owner.IsBot;
        public Vector3 OverlapPos { get; private set; }
        public float Radius { get; private set; }
        public int Damage { get; private set; }
        public NetworkId ProjectileId { get; private set; }


        public ProcessHitCollisionContext(NetworkId projectileId,
                                          Vector3 overlapPos, float radius, int damage,
                                          bool isHitFromServer, SessionPlayer owner)
        {
            IsHitFromServer = isHitFromServer;  
            Owner = owner;
            OverlapPos = overlapPos;
            Radius = radius;    
            Damage = damage;
            ProjectileId = projectileId;
        }
    }
}