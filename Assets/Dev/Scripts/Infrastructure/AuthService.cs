using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Dev
{
    public class AuthService : IDisposable
    {
        public static string Nickname;

        public async Task Auth()
        {
            await UnityServices.InitializeAsync();
            Debug.Log($"UGS initialized with state: {UnityServices.State}");

            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async Task UpdateNickname(string nickname)
        {
            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
            {
                Debug.Log($"Name is not assigned, setting player name");
                await AuthenticationService.Instance.UpdatePlayerNameAsync(nickname);
            }

            Nickname = AuthenticationService.Instance.PlayerName;
        }

        public bool IsNicknameNotSet() => string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName);

        public void LinkWithUsernameAndPassword(string username, string password)
        {
            AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
        }

        private void OnSignedIn()
        {
            Debug.Log($"Signed In");
        }

        private void OnSignInFailed(RequestFailedException obj)
        {
            Debug.Log($"Sign in Failed {obj}");
        }

        public void Dispose()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized) return;
            
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
        }
    }
}