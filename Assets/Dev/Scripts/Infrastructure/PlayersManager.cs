using System.Linq;
using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class PlayersManager : NetworkContext
    {
        [Networked, Capacity(10)] public NetworkLinkedList<PlayerData> Players { get; }

        public static PlayersManager Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void LoadLateInjection() { }

        public override void Spawned()
        {
            base.Spawned();

            if (HasStateAuthority)
            {
                RPC_Register(Object.StateAuthority, AuthService.Nickname);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_Register(PlayerRef playerRef, string nickname)
        {
            Players.Add(new PlayerData(nickname, playerRef));
        }

        public string GetNickname(PlayerRef playerRef)
        {
            return Players.First(x => x.PlayerRef == playerRef).Nickname.ToString();
        }
    }

    public struct PlayerData : INetworkStruct
    {
        public NetworkString<_32> Nickname { get; private set; }

        public PlayerRef PlayerRef { get; private set; }

        public PlayerData(string nickname, PlayerRef playerRef)
        {
            Nickname = nickname;
            PlayerRef = playerRef;
        }
    }
}