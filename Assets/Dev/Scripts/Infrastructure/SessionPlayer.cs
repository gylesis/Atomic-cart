using Dev.PlayerLogic;
using Fusion;
using Newtonsoft.Json;

namespace Dev.Infrastructure
{
    public struct SessionPlayer : INetworkStruct // the problem in what if some value will be changed somewhere outside  
    {
        private static readonly SessionPlayer @default = new SessionPlayer();
        
        public static SessionPlayer Default => default;
            
        [Networked]
        public NetworkId Id { get; private set; }
        

        [Networked]
        private NetworkString<_16> InternalName { get;  set; }

        public string Name => InternalName.ToString();
        
        [Networked]
        public NetworkBool IsBot { get; private set; }
        
        [Networked]
        public PlayerRef Owner { get; private set; }

        public SessionPlayer(NetworkId id, string name, bool isBot, PlayerRef owner)
        {   
            IsBot = isBot;
            Owner = owner;
            InternalName = name;    
            Id = id;
        }

    }

    public static class SessionPlayerExtensions
    {
        
    }

}