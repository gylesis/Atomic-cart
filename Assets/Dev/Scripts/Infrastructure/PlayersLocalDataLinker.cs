using System.Collections.Generic;
using System.Linq;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class PlayersLocalDataLinker : NetworkContext
    {
        [Networked, Capacity(10)] public NetworkLinkedList<PlayerData> Players { get; }

        private List<PlayerData> _localData = new List<PlayerData>();

        public static PlayersLocalDataLinker Instance { get; private set; }

        private SceneLoader _sceneLoader;

        [Inject]
        private void Construct(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }
        
        protected override void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            base.Awake();
        }
        
        protected override void Start()
        {
            base.Start();
            _sceneLoader.SceneLoaded.Subscribe((OnSceneLoaded)).AddTo(this);

            //transform.parent = FindObjectOfType<LobbyConnector>().transform;
        }

        private void OnSceneLoaded(string sceneName) // load
        {
            if (HasStateAuthority == false) return;
            
            if (sceneName == "Main")
            {
                foreach (var playerData in _localData)
                {
                    RPC_Register(playerData.PlayerRef, playerData.Nickname.ToString());
                }
            }
        }

        public override void Spawned()
        {
            base.Spawned();

            transform.parent = FindObjectOfType<LobbyConnector>().transform;
            RPC_Register(Runner.LocalPlayer, AuthService.Nickname);
        }

        [Rpc]
        public void RPC_Register(PlayerRef playerRef, string nickname)
        {
            if(Players.Any(x => x.PlayerRef == playerRef)) return;
            
            PlayerData playerData = new PlayerData(nickname, playerRef);

            Debug.Log($"Registered {nickname} for {playerRef}");
            
            Players.Add(playerData);
            //_localData.Add(playerData);
        }

        public string GetNickname(PlayerRef playerRef)
        {
            return Players.First(x => x.PlayerRef == playerRef).Nickname.ToString();
        }
    }

    public struct PlayerData : INetworkStruct
    {
        public NetworkString<_32> Nickname { get; private set; }
        public NetworkString<_32> FullNickname { get; private set; }
        public PlayerRef PlayerRef { get; private set; }
        

        public PlayerData(string nickname, PlayerRef playerRef)
        {
            FullNickname = nickname;
            Nickname = nickname.Split("#")[0];
            PlayerRef = playerRef;
        }
    }
}