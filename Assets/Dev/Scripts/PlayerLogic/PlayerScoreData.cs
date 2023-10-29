using Fusion;

namespace Dev.PlayerLogic
{
    public struct PlayerScoreData : INetworkStruct
    {
        [Networked] public PlayerRef PlayerId { get; set; }
        [Networked] public NetworkString<_16> Nickname { get; set; }
        [Networked] public TeamSide PlayerTeamSide { get; set; }
        [Networked] public int PlayerFragCount { get; set; }
        [Networked] public int PlayerDeathCount { get; set; }
    }
}