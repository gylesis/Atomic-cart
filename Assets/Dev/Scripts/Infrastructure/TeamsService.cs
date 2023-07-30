using System.Collections.Generic;
using System.Linq;
using Fusion;

namespace Dev.Infrastructure
{
    public class TeamsService : NetworkContext
    {
        private List<Team> _teams = new List<Team>();

       // public Team RedTeam => _teams.First(x => x.TeamSide == TeamSide.Red);
       // public Team BlueTeam => _teams.First(x => x.TeamSide == TeamSide.Blue);
        
        public override void Spawned()
        {
            if(HasStateAuthority == false) return;

            var blueTeam = new Team(TeamSide.Blue);
            var redTeam = new Team(TeamSide.Red);
            
            _teams.Add(blueTeam);
            _teams.Add(redTeam);
        }

        private Team GetTeamByMember(PlayerRef playerRef)
        {
            foreach (Team team in _teams)
            {
                var hasPlayer = team.HasPlayer(playerRef);

                if (hasPlayer)
                {
                    return team;
                }
            }

            return null;
        }

        public TeamSide GetPlayerTeamSide(PlayerRef playerRef)
        {
            return GetTeamByMember(playerRef).TeamSide;
        }
        
        public void AssignForTeam(PlayerRef playerRef, TeamSide teamSide)
        {
            Team team = _teams.First(x => x.TeamSide == teamSide);
            team.AddMember(playerRef);
        }

        public void RemoveFromTeam(PlayerRef playerRef)
        {
            Team team = GetTeamByMember(playerRef);
            
            team.RemoveMember(playerRef);
        }
        
    }
}