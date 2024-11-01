using System;
using System.Collections.Generic;
using Dev.Utils;
using Fusion;
using UniRx;
using Zenject;

namespace Dev.Infrastructure
{
    public class UnityDataLinker : NetworkContext
    {
        [Networked] public NetworkDictionary<PlayerRef, NetworkString<_64>> LinkData { get; }

        public static UnityDataLinker Instance { get; private set; }
        
        private Dictionary<PlayerRef, Profile> _profiles = new Dictionary<PlayerRef, Profile>();
        private AuthService _authService;

        public Subject<Unit> ProfilesFetched = new Subject<Unit>();

        public override void Spawned()
        {
            Instance = this;
            base.Spawned();
        }

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }

        [Rpc]
        public void RPC_Add(PlayerRef playerRef, string playerId)
        {
            LinkData.Add(playerRef, new NetworkString<_64>(playerId));
            FetchPlayersProfiles();
        }

        private async void FetchPlayersProfiles()
        {
            foreach (var pair in LinkData)
            {
                var playerRef = pair.Key;
                var playerId = pair.Value.ToString();

                if(_profiles.ContainsKey(playerRef)) continue;
                
                var tryGetProfile = await _authService.TryGetProfile(playerId);

                if (tryGetProfile.IsError)
                {
                    AtomicLogger.Err(tryGetProfile.ErrorMessage);
                    continue;
                }
                
                _profiles.Add(playerRef, tryGetProfile.Data);
            }
            
            ProfilesFetched.OnNext(Unit.Default);
        }

        public override void FixedUpdateNetwork()
        {
            foreach (var pair in LinkData)
            {
                AtomicLogger.Log($"Player {pair.Key} id {pair.Value.Value}");
            }
        }
       
        public string GetNickname(PlayerRef playerRef)
        {
            var nickname = _profiles.TryGetValue(playerRef, out var profile) ? profile.Nickname : "Unnamed";

            return nickname;
        }
    }
}