using System.Collections.Generic;
using Fusion;

namespace Dev.Infrastructure
{
    public class Team
    {
        private List<PlayerRef> _players = new List<PlayerRef>();
        public TeamSide TeamSide { get; private set; }

        public Team(TeamSide teamSide)
        {
            TeamSide = teamSide;
        }

        public void AddMember(PlayerRef playerRef)
        {
            _players.Add(playerRef);
        }

        public void RemoveMember(PlayerRef playerRef)
        {
            _players.Remove(playerRef);
        }
    
    }
}