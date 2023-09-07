﻿using System.Linq;
using Fusion;

namespace Dev.Infrastructure
{
    public class TeamsService : NetworkContext
    {
        [Networked, Capacity(2)] private NetworkLinkedList<Team> Teams { get; }

       // public Team RedTeam => _teams.First(x => x.TeamSide == TeamSide.Red);
       // public Team BlueTeam => _teams.First(x => x.TeamSide == TeamSide.Blue);


       public override void Spawned()
        {
            if(HasStateAuthority == false) return;

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
            
            team.RemoveMember(playerRef);
        }
        
    }
}