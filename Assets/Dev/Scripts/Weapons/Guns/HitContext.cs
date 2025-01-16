using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public struct HitContext
    {
        public GameObject GameObject { get; private set;}
        public DamagableType DamagableType { get; private set;}
        public NetworkId ProjectileId { get; private set;}
        
        public HitContext(GameObject gameObject, DamagableType damagableType, NetworkId projectileId)
        {
            GameObject = gameObject;
            DamagableType = damagableType;
            ProjectileId = projectileId;
        }
    }
}