using System.Linq;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Utils;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class TeamsService : NetworkContext
    {
        private SessionStateService _sessionStateService;
        [Networked, Capacity(2)] private NetworkLinkedList<Team> Teams { get; }

        [Inject]
        private void Construct(SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
        }

        public override void Spawned()
        {
            base.Spawned();
            
            if (Runner.IsSharedModeMasterClient == false) return;

            var blueTeam = new Team(TeamSide.Blue);
            var redTeam = new Team(TeamSide.Red);

            Teams.Add(blueTeam);
            Teams.Add(redTeam);
        }

        public int GetTeamMembersCount(TeamSide teamSide)
        {
            return GetTeam(teamSide).MembersCount;
        }

        private Team GetMemberTeam(NetworkId memberId)
        {
            foreach (Team team in Teams)
            {
                var hasPlayer = team.HasTeamMember(memberId);

                if (hasPlayer)
                    return team;
            }

            Debug.Log($"Didnt found team for {memberId}");

            return Teams.First();
        }

        public bool DoPlayerHasTeam(SessionPlayer player)
        {
            return Teams.Any(x => x.HasTeamMember(player));
        }
        
        public bool DoPlayerHasTeam(NetworkId networkId)
        {
            return Teams.Any(x => x.HasTeamMember(networkId));
        }
        
        public bool TryGetUnitTeamSide(SessionPlayer player, out TeamSide teamSide)
        {
            teamSide = TeamSide.None;
            
            if (DoPlayerHasTeam(player))
            {   
                teamSide = GetMemberTeam(player.Id).TeamSide;
                return true;
            }

            return false;
        }
        
        public bool TryGetUnitTeamSide(PlayerRef playerRef, out TeamSide teamSide)
        {   
            teamSide = TeamSide.None;
            
            if (DoPlayerHasTeam(playerRef.ToNetworkId()))
            {   
                teamSide = GetMemberTeam(playerRef.ToNetworkId()).TeamSide;
                return true;
            }

            return false;
        }
        
        public bool TryGetUnitTeamSide(NetworkId networkId, out TeamSide teamSide)
        {       
            teamSide = TeamSide.None;
            
            if (DoPlayerHasTeam(networkId))
            {   
                teamSide = GetMemberTeam(networkId).TeamSide;
                return true;
            }

            return false;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_AssignForTeam(TeamMember teamMember, TeamSide teamSide)
        {
            Team team = GetTeam(teamSide);
            int indexOf = Teams.IndexOf(team);

            team.AddMember(teamMember);

            if (!teamMember.IsPlayer)
            {
                var sessionPlayer = _sessionStateService.GetSessionPlayer(teamMember.MemberId);
                if (_sessionStateService.TryGetBot(sessionPlayer, out var bot)) bot.UpdateTeam(teamSide);
            }

            AtomicLogger.Log($"Player {teamMember.MemberId} Added to {team.TeamSide}");

            Teams.Set(indexOf, team);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RemoveFromTeam(NetworkId memberId)
        {
            var hasTeam = DoPlayerHasTeam(memberId);

            if (!hasTeam)
            {
                AtomicLogger.Log($"Nowhere to remove member: {memberId}");
                return;
            }

            Team team = GetMemberTeam(memberId);
            int indexOf = Teams.IndexOf(team);

            team.RemoveMember(memberId);

            AtomicLogger.Log($"Player {memberId} removed from {team.TeamSide}");
            
            Teams.Set(indexOf, team);
        }

        public void SwapTeams()
        {
            Team blueTeam = GetTeam(TeamSide.Blue);
            Team redTeam = GetTeam(TeamSide.Red);

            var blueTeamMembers = blueTeam.MembersList;
            var redTeamMembers = redTeam.MembersList;
            
            foreach (TeamMember blueTeamPlayer in blueTeamMembers)
            {   
                RPC_RemoveFromTeam(blueTeamPlayer.MemberId);
                RPC_AssignForTeam(blueTeamPlayer, TeamSide.Red);
            }

            foreach (TeamMember redTeamPlayer in redTeamMembers)
            {
                RPC_RemoveFromTeam(redTeamPlayer.MemberId);
                RPC_AssignForTeam(redTeamPlayer, TeamSide.Blue);
            }
        }

        private Team GetTeam(TeamSide teamSide) => Teams.First(x => x.TeamSide == teamSide);
    }
}