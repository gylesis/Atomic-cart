using Dev.Infrastructure;
using Fusion;

namespace Dev
{
    public struct HitRecord : INetworkStruct
    {   
        public SessionPlayer Owner { get; private set; }
        public SessionPlayer Victim { get; private set; }
        public NetworkBool IsServer { get; private set; }
        public long HitTime { get; private set;}
        public float Damage { get; private set; }

        public HitRecord(SessionPlayer owner, SessionPlayer victim, float damage, long hitTime, bool isServer)
        {
            HitTime = hitTime;    
            IsServer = isServer;
            Owner = owner;
            Victim = victim;
            Damage = damage;
        }
    }
}