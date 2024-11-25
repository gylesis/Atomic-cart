using Cysharp.Threading.Tasks;
using Dev.Utils;
using Fusion;
using UniRx;
using Zenject;

namespace Dev.Infrastructure
{
    public class UnityDataLinker : NetSingleton<UnityDataLinker>
    {
        [Networked] public NetworkDictionary<PlayerRef, NetworkString<_64>> LinkData { get; } // PlayerRef to PlayerID map

        private AuthService _authService;

        public Subject<Unit> ProfilesFetched = new Subject<Unit>();

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
                PlayerRef playerRef = pair.Key;
                string playerId = pair.Value.ToString();

                var tryGetProfile = await _authService.GetProfileAsync(playerId);

                if (tryGetProfile.IsError)
                {
                    AtomicLogger.Err(tryGetProfile.ErrorMessage, AtomicConstants.LogTags.Networking);
                    continue;
                }
                
                _authService.GetProfileAsync(playerId).Forget(); // only caches once, not new data
            }
            
            ProfilesFetched.OnNext(Unit.Default);
        }

        public override void Render()
        {
            base.Render();
            
            foreach (var pair in LinkData)
            {
                string playerID = pair.Value.ToString();

                string nickname = "undefined";

                if(_authService.TryGetCachedProfile(playerID, out var profile)) 
                    nickname = profile.Nickname;

                AtomicLogger.Log($"Player {pair.Key}, nickname: {nickname}");
            }
        }
       
        public string GetNickname(PlayerRef playerRef)
        {
            string playerId = LinkData[playerRef].ToString();
            var nickname = _authService.TryGetCachedProfile(playerId, out var profile) ? profile.Nickname : "Unnamed";
            return nickname;
        }

        public async UniTask<string> GetNicknameAsync(PlayerRef playerRef)
        {
            string playerId = LinkData[playerRef].ToString();
            var result = await _authService.GetNicknameAsync(playerId);
            
            if(result.IsError)
                return "unnamed";

            return result.Data;
        }
    }
}