using Cysharp.Threading.Tasks;
using Dev.UI;
using Dev.Utils;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure.Networking
{
    public class SaveDataModule : IInitializableModule
    {
        private SaveLoadService _saveLoadService;
        private InternetChecker _internetChecker;
        private AuthService _authService;

        [Inject]
        private void Construct(SaveLoadService saveLoadService, InternetChecker internetChecker, AuthService authService)
        {
            _authService = authService;
            _saveLoadService = saveLoadService;
        }

        public bool IsInitialized { get; private set; }

        public async UniTask<Result> Initialize()
        {
            Curtains.Instance.SetText("Loading player data");
            Curtains.Instance.ShowWithDotAnimation(0);
            
            AtomicLogger.Log($"Loading player data");
            
            Profile profile = await _saveLoadService.Load();

            bool isNewPlayer = string.IsNullOrEmpty(profile.Nickname);
            
            if (isNewPlayer)
            {
                profile.Nickname = $"Player{Random.Range(1, 100)}";

                var saveResult = await _saveLoadService.Save(newProfile =>
                {
                    newProfile.Nickname = profile.Nickname;
                });
                
                if(saveResult.IsError)
                    return saveResult;
            }
            
            AtomicLogger.Log($"Player ID {_authService.PlayerId}, nickname {profile.Nickname}", AtomicConstants.LogTags.Networking);
            
            IsInitialized = true;
            return Result.Success();
        }
    }
}