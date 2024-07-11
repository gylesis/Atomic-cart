using Dev.Infrastructure;
using Fusion;

namespace Dev.PlayerLogic
{
    public class PlayerCharacterClassChangeService
    {
        private PlayersSpawner _playersSpawner;

        public PlayerCharacterClassChangeService(PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
        }

        public void ChangePlayerCharacterClass(PlayerRef playerRef, CharacterClass characterClass)
        {
            _playersSpawner.ChangePlayerCharacter(playerRef, characterClass);
        }
        
    }
}