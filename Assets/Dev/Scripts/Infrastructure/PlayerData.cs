using Fusion;

namespace Dev.Infrastructure
{
    public struct PlayerData : INetworkStruct
    {
        public NetworkString<_32> Nickname { get; private set; }
        public NetworkString<_32> FullNickname { get; private set; }
        public PlayerRef PlayerRef { get; private set; }
        

        public PlayerData(string nickname, PlayerRef playerRef)
        {
            FullNickname = nickname;
            Nickname = nickname.Split("#")[0];
            PlayerRef = playerRef;
        }
    }
}