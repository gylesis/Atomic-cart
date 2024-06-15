using System.Linq;
using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class TeamsService : NetworkContext
    {
        [Networked, Capacity(2)] private NetworkLinkedList<Team> Teams { get; }

        // public Team RedTeam => _teams.First(x => x.TeamSide == TeamSide.Red);
        // public Team BlueTeam => _teams.First(x => x.TeamSide == TeamSide.Blue);


        public override void Spawned()
        {
            //Debug.Log($"SDFs");
            
            if (HasStateAuthority == false) return;

            var blueTeam = new Team(TeamSide.Blue);
            var redTeam = new Team(TeamSide.Red);

            Teams.Add(blueTeam);
            Teams.Add(redTeam);
        }

        private Team GetTeamByMember(TeamMember teamMember)
        {
            foreach (Team team in Teams)
            {
                var hasPlayer = team.HasTeamMember(teamMember);

                if (hasPlayer)
                {
                    return team;
                }
            }

            Debug.Log($"Didnt found team for {teamMember.MemberId}");

            return Teams.First();
        }

        public bool DoPlayerHasTeam(PlayerRef playerRef)
        {
            return Teams.Any(x => x.HasTeamMember(playerRef));
        }
        
        public TeamSide GetUnitTeamSide(TeamMember teamMember)
        {
            return GetTeamByMember(teamMember).TeamSide;
        }
        
        public void AssignForTeam(TeamMember teamMember, TeamSide teamSide)
        {
            Team team = Teams.First(x => x.TeamSide == teamSide);
            int indexOf = Teams.IndexOf(team);

            team.AddMember(teamMember);

            Teams.Set(indexOf, team);
        }

        public void RemoveFromTeam(TeamMember teamMember)
        {
            Team team = GetTeamByMember(teamMember);
            int indexOf = Teams.IndexOf(team);

            team.RemoveMember(teamMember);

            Teams.Set(indexOf, team);
        }

        public void SwapTeams()
        {
            Team blueTeam = Teams.FirstOrDefault(x => x.TeamSide == TeamSide.Blue);
            Team redTeam = Teams.FirstOrDefault(x => x.TeamSide == TeamSide.Red);

            foreach (TeamMember blueTeamPlayer in blueTeam.Players)
            {   
                RemoveFromTeam(blueTeamPlayer);
                AssignForTeam(blueTeamPlayer, TeamSide.Red);
            }

            foreach (TeamMember redTeamPlayer in redTeam.Players)
            {
                RemoveFromTeam(redTeamPlayer);
                AssignForTeam(redTeamPlayer, TeamSide.Blue);
            }
        }

        
    }
}