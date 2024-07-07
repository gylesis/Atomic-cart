using Dev.PlayerLogic;
using Fusion;

namespace Dev.Infrastructure
{
    public struct SessionPlayer : INetworkStruct
    {
        [Networked]
        public NetworkId Id { get; private set; }

        [Networked]
        private NetworkString<_16> InternalName { get;  set; }

        public string Name => InternalName.ToString();
        
        [Networked]
        public NetworkBool IsBot { get; private set; }
        
        public TeamSide TeamSide { get; private set; }
        
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