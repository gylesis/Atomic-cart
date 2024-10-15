using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Fusion;

namespace Dev.PlayerLogic
{
    public struct Team : INetworkStruct
    {
        [Networked, Capacity(10)] private NetworkLinkedList<TeamMember> Members => default;
        [Networked] public TeamSide TeamSide { get; private set; }
        
        public IEnumerable<TeamMember> MembersList => Members.AsEnumerable();
        public int MembersCount => Members.Count;
    
        public Team(TeamSide teamSide)
        {
            TeamSide = teamSide;
        }
        
        public bool HasTeamMember(NetworkId memberId)
        {
            return Members.Any(x => x.MemberId == memberId);
        }
        public bool HasTeamMember(SessionPlayer sessionPlayer)
        {
            return Members.Any(x => x.MemberId == sessionPlayer.Id);
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
            if (!HasTeamMember(networkId)) return;
            
            TeamMember teamMember = Members.FirstOrDefault(x => x.MemberId == networkId);
            Members.Remove(teamMember);
        }
     
    }
}   