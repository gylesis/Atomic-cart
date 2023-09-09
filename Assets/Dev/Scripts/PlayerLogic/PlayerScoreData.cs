using Fusion;

namespace Dev.PlayerLogic
{
    public class PlayerScoreData : INetworkStruct
    {
        public PlayerRef PlayerId;
        public string Nickname;
        public TeamSide PlayerTeamSide;
        public int PlayerFragCount;
        public int PlayerDeathCount;
    }
}