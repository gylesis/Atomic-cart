using System;
using Fusion;

namespace Dev
{
    public struct ObjectWithHealthData : INetworkStruct
    {
        [Networked] public UInt16 Health { get; private set; }
        [Networked] public UInt16 MaxHealth { get; private set; }
        [Networked] public NetworkId ObjId { get; private set; }
        
        public ObjectWithHealthData(NetworkId objId, UInt16 health, UInt16 maxHealth)
        {
            Health = health;
            MaxHealth = maxHealth;
            ObjId = objId;
        }
    }
}