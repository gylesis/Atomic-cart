using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersDataService : NetworkContext
    {
        private PlayersSpawner _playersSpawner;
        private SessionStateService _sessionStateService;
        
        public PlayersSpawner PlayersSpawner => _playersSpawner;

        [Inject]
        public void Init(SessionStateService sessionStateService, PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
            _sessionStateService = sessionStateService;
        }
        
        public string GetNickname(PlayerRef playerRef)
        {
            return _sessionStateService.GetSessionPlayer(playerRef).Name;
        }
        
        public PlayerCharacter GetPlayer(PlayerRef playerRef)
        {
            return _playersSpawner.GetPlayer(playerRef);
        }
        
        public PlayerBase GetPlayerBase(PlayerRef playerRef)
        {
            return _playersSpawner.GetPlayerBase(playerRef);
        }
        
        public PlayerBase GetPlayerBase(NetworkId id)
        {
            return _playersSpawner.GetPlayerBase(id);
        }
        
        public CharacterClass GetPlayerCharacterClass(PlayerRef playerRef)
        {
            return _playersSpawner.GetPlayerBase(playerRef).CharacterClass;
        }
        
        public Vector3 GetPlayerPos(PlayerRef playerRef) => GetPlayer(playerRef).transform.position;
    }
    
}