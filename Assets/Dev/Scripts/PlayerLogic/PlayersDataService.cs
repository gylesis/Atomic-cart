using System;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersDataService : NetworkContext
    {
        private PlayersSpawner _playersSpawner;

        [Networked, Capacity(20)] private NetworkDictionary<PlayerRef, PlayerData> PlayersHealth { get; }

        public static PlayersDataService Instance { get; private set; }

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

        private void Start()
        {
            _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
        }

        public string GetNickname(PlayerRef playerRef)
        {
            return PlayersHealth[playerRef].Name.Value;
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            PlayerRef playerRef = spawnEventContext.PlayerRef;

            var playerData = new PlayerData(playerRef, $"Player {playerRef.PlayerId}");

            PlayersHealth.Add(playerRef, playerData);
        }
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