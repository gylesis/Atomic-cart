using Dev.BotsLogic;
using Dev.Utils;
using Fusion;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public struct TeamMember : INetworkStruct
    {
        [Networked] public NetworkId MemberId { get; private set; }


        public bool IsPlayer { get; private set; }
        
        public TeamMember(Bot bot)
        {
            MemberId = bot.Object;
            IsPlayer = false;
        }
        
        public TeamMember(PlayerRef memberId)
        {
            MemberId = memberId.ToNetworkId();
            IsPlayer = true;
        }

    }
}