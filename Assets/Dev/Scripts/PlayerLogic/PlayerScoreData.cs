using Dev.Infrastructure;
using Fusion;

namespace Dev.PlayerLogic
{
    public struct PlayerScoreData : INetworkStruct
    {
        [Networked]
        public SessionPlayer SessionPlayer { get; private set; }
        [Networked] public int PlayerFragCount { get; set; }
        [Networked] public int PlayerDeathCount { get; set; }

        public PlayerScoreData(SessionPlayer sessionPlayer, int playerFragCount = 0, int playerDeathCount = 0)
        {
            SessionPlayer = sessionPlayer;
            PlayerFragCount = playerFragCount;
            PlayerDeathCount = playerDeathCount;
        }
    }
}