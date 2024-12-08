using Cysharp.Threading.Tasks;
using Dev.UI;
using Dev.Utils;
using Unity.Services.Core;
using Zenject;

namespace Dev.Infrastructure.Networking
{
    public class ServicesModule : IInitializableModule
    {
        private AuthService _authService;
        
        public bool IsInitialized { get; private set; }
        
        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService; 
        }
        
        public async UniTask<Result> Initialize()
        {
            if (UnityServices.State != ServicesInitializationState.Uninitialized) return Result.Success();
            
            Curtains.Instance.SetText("Initializing services");
            Curtains.Instance.ShowWithDotAnimation(0);
            
            await UnityServices.InitializeAsync();
            AtomicLogger.Log($"UGS initialized with state: {UnityServices.State}",
                AtomicConstants.LogTags.Networking);

            Curtains.Instance.SetText("Authorizing");
            Curtains.Instance.ShowWithDotAnimation(0);

            var authResult = await _authService.Auth();   

            if(authResult.IsError)
                return authResult;
            
            IsInitialized = true;
            return Result.Success();
        }
    }
}   