using Fusion;

namespace Dev.Infrastructure
{
    public class TeamsService : NetworkContext
    {
        private Team _blueTeam;
        private Team _redTeam;

        public override void Spawned()
        {
            if(HasStateAuthority == false) return;

            _blueTeam = new Team(TeamSide.Blue);
            _redTeam = new Team(TeamSide.Red);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_AssignForTeam(PlayerRef playerRef, TeamSide teamSide)
        {
            switch (teamSide)
            {
                case TeamSide.Blue:
                    _blueTeam.AddMember(playerRef);
                    break;
                case TeamSide.Red:
                    _redTeam.AddMember(playerRef);
                    break;
            }
        }
        
    }
}