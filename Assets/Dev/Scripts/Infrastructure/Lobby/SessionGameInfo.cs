namespace Dev.Infrastructure.Lobby
{
    public struct SessionGameInfo
    {
        public int Id;
        public string SessionName;
        public int CurrentPlayers;
        public int MaxPlayers;
        public string MapName;
        public MapType MapType;
        public SessionStatus SessionStatus;
    }
}