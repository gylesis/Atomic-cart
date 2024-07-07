using System.Linq;
using Dev.BotsLogic;
using Fusion;

namespace Dev.PlayerLogic
{
    public struct Team : INetworkStruct
    {
        [Networked, Capacity(10)] public NetworkLinkedList<TeamMember> Members => default;
        [Networked] public TeamSide TeamSide { get; private set; }
    
        public Team(TeamSide teamSide)
        {
            TeamSide = teamSide;
        }

        public bool HasTeamMember(TeamMember teamMember)
        {
            return Members.Any(x => x.MemberId == teamMember.MemberId);
        }
        
        public void AddMember(TeamMember teamMember)
        {
            Members.Add(teamMember);
        }   

        public void RemoveMember(TeamMember teamMember)
        { 
            Members.Remove(teamMember);
        }
        
        public void RemoveMember(NetworkId networkId)
        {
            bool hasMember = Members.Any(x => x.MemberId == networkId.Raw);

            if (hasMember != null)
            {
                TeamMember teamMember = Members.FirstOrDefault(x => x.MemberId == networkId.Raw);

                Members.Remove(teamMember);
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