using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;

namespace Dev.BotsLogic
{
    public struct BotData : INetworkStruct
    {
        [Networked]
        public SessionPlayer SessionPlayer { get; private set; }
        
        [Networked]
        public CharacterClass CharacterClass { get; private set; }

        public BotData(SessionPlayer sessionPlayer, CharacterClass characterClass)
        {
            SessionPlayer = sessionPlayer;
            CharacterClass = characterClass;
        }
    }
}