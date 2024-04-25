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

        [Networked, Capacity(20)] private NetworkDictionary<PlayerRef, PlayerData> PlayersData { get; }

        public static PlayersDataService Instance { get; private set; }

        public PlayersSpawner PlayersSpawner => _playersSpawner;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        [Inject]
        public void Init(PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _playersSpawner.PlayerSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            _playersSpawner.PlayerDeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));
        }

        public string GetNickname(PlayerRef playerRef)
        {
            return PlayersData[playerRef].Name.Value;
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            PlayerRef playerRef = spawnEventContext.PlayerRef;

            var playerData = new PlayerData(playerRef, $"Player {playerRef.PlayerId}");

            PlayersData.Add(playerRef, playerData);
        }
        
        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            PlayersData.Remove(playerRef);
        }

        public PlayerCharacter GetPlayer(PlayerRef playerRef)
        {
            return _playersSpawner.GetPlayer(playerRef);
        }

        public CharacterClass GetPlayerCharacterClass(PlayerRef playerRef)
        {
            return GetPlayer(playerRef).CharacterClass;
        }
        
        public Vector3 GetPlayerPos(PlayerRef playerRef) => GetPlayer(playerRef).transform.position;
    }
    
    public struct PlayerData : INetworkStruct
    {
        [Networked] public NetworkString<_16> Name { get; set; }
        [Networked] public PlayerRef PlayerRef { get; set; }

        public PlayerData(PlayerRef playerRef, string name)
        {
            PlayerRef = playerRef;
            Name = name;
        }
    }
}