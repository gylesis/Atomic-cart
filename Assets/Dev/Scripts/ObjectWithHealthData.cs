using System;
using Fusion;

namespace Dev
{
    public struct ObjectWithHealthData : INetworkStruct
    {
        [Networked] public UInt16 Health { get; private set; }
        [Networked] public NetworkId ObjId { get; private set; }
        
        public ObjectWithHealthData(NetworkId objId, UInt16 health)
        {
            Health = health;
            ObjId = objId;
        }
    }
}