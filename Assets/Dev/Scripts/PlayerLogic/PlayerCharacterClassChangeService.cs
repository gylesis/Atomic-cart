﻿using Dev.Infrastructure;
using Fusion;

namespace Dev.PlayerLogic
{
    public class PlayerCharacterClassChangeService
    {
        private NetworkRunner _runner;
        private PlayersSpawner _playersSpawner;

        public PlayerCharacterClassChangeService(NetworkRunner runner, PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
            _runner = runner;
        }

        public void ChangePlayerCharacterClass(PlayerRef playerRef, CharacterClass characterClass)
        {
            _playersSpawner.ChangePlayerCharacter(playerRef, characterClass, _runner);

            
            
        }
        
    }
}