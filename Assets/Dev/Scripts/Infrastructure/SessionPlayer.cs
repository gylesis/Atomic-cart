using Dev.PlayerLogic;
using Fusion;
using Newtonsoft.Json;

namespace Dev.Infrastructure
{
    public struct SessionPlayer : INetworkStruct
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
        public TeamSide TeamSide { get; private set; }
        
        [Networked]
        public PlayerRef Owner { get; private set; }

        public SessionPlayer(NetworkId id, string name, bool isBot, TeamSide teamSide, PlayerRef owner)
        {   
            IsBot = isBot;
            TeamSide = teamSide;
            Owner = owner;
            InternalName = name;    
            Id = id;
        }
    }

}