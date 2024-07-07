using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;

namespace Dev.BotsLogic
{
    public struct BotData : INetworkStruct
    {
        public SessionPlayer SessionPlayer { get; private set; }
        public CharacterClass CharacterClass { get; private set; }

        public TeamSide TeamSide => SessionPlayer.TeamSide;

        public BotData(SessionPlayer sessionPlayer, CharacterClass characterClass)
        {
            SessionPlayer = sessionPlayer;
            CharacterClass = characterClass;
        }
    }
}