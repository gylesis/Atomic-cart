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
            
            _playersSpawner.PlayerBaseSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerBaseSpawned));
            _playersSpawner.PlayerBaseDeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));
        }

        public bool HasData(PlayerRef playerRef)
        {
            return PlayersData.ContainsKey(playerRef);
        }
        
        public string GetNickname(PlayerRef playerRef)
        {
            return PlayersData[playerRef].Name.Value;
        }

        private void OnPlayerBaseSpawned(PlayerSpawnEventContext spawnEventContext)
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