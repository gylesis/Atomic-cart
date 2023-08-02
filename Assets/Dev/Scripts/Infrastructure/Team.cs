using Fusion;

namespace Dev.Infrastructure
{
    public struct Team : INetworkStruct
    {
        [Networked, Capacity(10)] private NetworkLinkedList<PlayerRef> Players => default;
        [Networked] public TeamSide TeamSide { get; private set; }

        public Team(TeamSide teamSide)
        {
            TeamSide = teamSide;
        }

        public bool HasPlayer(PlayerRef playerRef) => Players.Contains(playerRef);
        
        public void AddMember(PlayerRef playerRef)
        {
            Players.Add(playerRef);
        }

        public void RemoveMember(PlayerRef playerRef)
        {
            Players.Remove(playerRef);
        }
    
    }
}