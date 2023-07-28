﻿using System;
using Dev.Infrastructure;
using Fusion;
using UniRx;

namespace Dev
{
    public class PlayersDataService : NetworkContext
    {
        private PlayersSpawner _playersSpawner;

        [Networked] private NetworkDictionary<PlayerRef, PlayerData> PlayersHealth { get; }

        public static PlayersDataService Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void Init(PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
        }

        public override void Spawned()
        {
            if (HasStateAuthority == false) return;

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