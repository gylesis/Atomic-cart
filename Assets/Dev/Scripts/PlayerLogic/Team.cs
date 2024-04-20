using System.Linq;
using Dev.BotsLogic;
using Fusion;

namespace Dev.PlayerLogic
{
    public struct Team : INetworkStruct
    {
        [Networked, Capacity(10)] public NetworkLinkedList<TeamMember> Players => default;
        [Networked] public TeamSide TeamSide { get; private set; }
    
        public Team(TeamSide teamSide)
        {
            TeamSide = teamSide;
        }

        public bool HasTeamMember(TeamMember teamMember)
        {
            return Players.Any(x => x.MemberId == teamMember.MemberId);
        }
        
        public void AddMember(TeamMember teamMember)
        {
            Players.Add(teamMember);
        }   

        public void RemoveMember(TeamMember teamMember)
        { 
            Players.Remove(teamMember);
        }
        
        public void RemoveMember(NetworkId networkId)
        {
            bool hasMember = Players.Any(x => x.MemberId == networkId.Raw);

            if (hasMember != null)
            {
                TeamMember teamMember = Players.FirstOrDefault(x => x.MemberId == networkId.Raw);

                Players.Remove(teamMember);
            }
        }
    }

    public struct TeamMember : INetworkStruct
    {
        [Networked] public uint MemberId { get; private set; }

        public TeamMember(uint memberId)
        {
            MemberId = memberId;
        }
        
        public TeamMember(int memberId)
        {
            MemberId = (uint)memberId;
        }

        public static implicit operator TeamMember(PlayerRef playerRef)
        {
            return new TeamMember(playerRef.PlayerId);
        }
        
        public static implicit operator TeamMember(NetworkId networkId)
        {
            return new TeamMember(networkId.Raw);
        }
        
        public static implicit operator TeamMember(Bot bot)
        {
            return new TeamMember(bot.Id.Object.Raw);
        }

    }   
}   