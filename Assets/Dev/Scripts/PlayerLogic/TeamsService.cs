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

        private Team GetTeamByMember(PlayerRef playerRef)
        {
            foreach (Team team in Teams)
            {
                var hasPlayer = team.HasPlayer(playerRef);

                if (hasPlayer)
                {
                    return team;
                }
            }

            return Teams.First();
        }

        public bool DoPlayerHasTeam(PlayerRef playerRef)
        {
            return Teams.Any(x => x.HasPlayer(playerRef));
        }
        
        public TeamSide GetPlayerTeamSide(PlayerRef playerRef)
        {
            return GetTeamByMember(playerRef).TeamSide;
        }

        public void AssignForTeam(PlayerRef playerRef, TeamSide teamSide)
        {
            Team team = Teams.First(x => x.TeamSide == teamSide);
            int indexOf = Teams.IndexOf(team);

            team.AddMember(playerRef);

            Teams.Set(indexOf, team);
        }

        public void RemoveFromTeam(PlayerRef playerRef)
        {
            Team team = GetTeamByMember(playerRef);
            int indexOf = Teams.IndexOf(team);

            team.RemoveMember(playerRef);

            Teams.Set(indexOf, team);
        }

        public void SwapTeams()
        {
            Team blueTeam = Teams.FirstOrDefault(x => x.TeamSide == TeamSide.Blue);
            Team redTeam = Teams.FirstOrDefault(x => x.TeamSide == TeamSide.Red);

            foreach (PlayerRef blueTeamPlayer in blueTeam.Players)
            {
                RemoveFromTeam(blueTeamPlayer);
                AssignForTeam(blueTeamPlayer, TeamSide.Red);
            }

            foreach (PlayerRef redTeamPlayer in redTeam.Players)
            {
                RemoveFromTeam(redTeamPlayer);
                AssignForTeam(redTeamPlayer, TeamSide.Blue);
            }
        }
    }
}