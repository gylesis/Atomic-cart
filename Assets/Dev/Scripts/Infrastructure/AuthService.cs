using System;
using System.Threading.Tasks;
using Dev.Utils;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Dev
{
    public class AuthService : IDisposable
    {
        public static string Nickname;

        public string Token;
        public string Error;

        public static bool IsAuthorized { get; private set; }
        
        public async Task<bool> Auth()
        {   
            //await LoginGooglePlayGames();
            //await SignInWithGooglePlayGamesAsync(Token);
            
            //AuthenticationService.Instance.SignedIn += OnSignedIn;
            //AuthenticationService.Instance.SignInFailed += OnSignInFailed;

            //return true;
            
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                IsAuthorized = true;
                
                /*if (AuthenticationService.Instance.SessionTokenExists == false)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                else
                {
                    string username = PlayerPrefs.GetString("username", "gylesis");
                    string password = PlayerPrefs.GetString("pass", "qwertY1!");
                    
                    await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                }*/
                
                return true;
            }
            catch (Exception e)
            {
                IsAuthorized = false;
                AtomicLogger.Ex(e.Message);
                return false;
            }
        }
        
        public Task<bool> LoginGooglePlayGames()
        {
            var tcs = new TaskCompletionSource<bool>();
            
            PlayGamesPlatform.Instance.Authenticate((success) =>
            {
                if (success == SignInStatus.Success)
                {
                    AtomicLogger.Log("Login with Google Play games successful.");
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                    {
                        Debug.Log("Authorization code: " + code);
                        Token = code;
                        // This token serves as an example to be used for SignInWithGooglePlayGames

                        tcs.SetResult(true);
                    });
                }
                else
                {
                    Error = "Failed to retrieve Google play games authorization code";
                    AtomicLogger.Log("Login Unsuccessful");
                    tcs.SetResult(false);
                }
            });
            
            return tcs.Task;
        }
        
        
        private async Task<bool> SignInWithGooglePlayGamesAsync(string authCode)
        {
            try
            {   
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}"); //Display the Unity Authentication PlayerID
                Debug.Log("SignIn is successful.");
                return true;
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                AtomicLogger.Ex(ex.Message);
                return false;
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                AtomicLogger.Ex(ex.Message);

                return false;
            }
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