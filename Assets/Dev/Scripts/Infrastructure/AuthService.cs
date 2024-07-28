using System;
using System.Linq;
using System.Threading.Tasks;
using Dev.Utils;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Dev
{
    public class AuthService : IDisposable
    {
        public static string Nickname;

        public async Task<bool> Auth()
        {   
            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;
            
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync("gylesis", "qwertY1!");
                return true;
            }
            catch (Exception e)
            {
                AtomicLogger.Ex(e.Message);
                return false;
            }
           // await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async Task UpdateNickname(string nickname)
        {
            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
            {
                AtomicLogger.Log($"Name is not assigned, setting player name", AtomicConstants.LogTags.Networking);
                await AuthenticationService.Instance.UpdatePlayerNameAsync(nickname);
            }
            
            Nickname = AuthenticationService.Instance.PlayerName;
        }

        public bool IsNicknameNotSet => string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName);
    
        public async Task<bool> LinkWithUsernameAndPassword(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
                return true;
            }
            catch (Exception e)
            {
                AtomicLogger.Ex(e.Message);
                return false;
            }
        }

        private void OnSignedIn()
        {
            AtomicLogger.Log($"Signed In");
        }

        private void OnSignInFailed(RequestFailedException exception)
        {
            AtomicLogger.Log($"Sign in Failed {exception}");
        }

        public void Dispose()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized) return;
            
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
        }
    }
}