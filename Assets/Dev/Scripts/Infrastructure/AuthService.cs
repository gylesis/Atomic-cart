using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Utils;
#if !UNITY_STANDALONE_WIN
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Dev
{
    public class AuthService : IDisposable
    {
        public string Token;
        public string Error;
        
        private SaveLoadService _saveLoadService;

        public static bool IsAuthorized { get; private set; }

        public AuthService(SaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
        }
        
        public async Task<bool> Auth()
        {   
            //await LoginGooglePlayGames();
            //await SignInWithGooglePlayGamesAsync(Token);
            
            AuthenticationService.Instance.SignedIn += OnSignedIn; 
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;
            
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                IsAuthorized = true;
                return true;
            }
            catch (Exception e)
            {
                IsAuthorized = false;
                AtomicLogger.Ex(e.Message);
                return false;
            }
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


        public async UniTask<Result> UpdateNickname(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
            {
                AtomicLogger.Err("Nickname cannot be empty.");
                //AtomicLogger.Log($"Name is not assigned, setting player name", AtomicConstants.LogTags.Networking);
                return Result.Error("Nickname cannot be empty.");
            }

            await AuthenticationService.Instance.UpdatePlayerNameAsync(nickname);
            await _saveLoadService.Save(profile => profile.Nickname = nickname);
            
            return Result.Success();
        }

        public bool IsNicknameNotSet => string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName);

        public async Task<Result<Profile>> TryGetProfile(string playerId)
        {
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{"profile"}, new LoadOptions(new PublicReadAccessClassOptions(playerId)));

            bool tryGetValue = playerData.TryGetValue("profile", out Item profileItem);

            if (tryGetValue)
            {       
                var asString = profileItem.Value.GetAsString();
                return Result<Profile>.Success(JsonUtility.FromJson<Profile>(asString));
            }

            return Result<Profile>.Error($"No profile found for player {playerId}.");
        }
        
        public async UniTask DeleteAccount()
        {
            await AuthenticationService.Instance.DeleteAccountAsync();
        }

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
            AtomicLogger.Err($"Sign in Failed: {exception.Message}");
        }
        
        
#if !UNITY_STANDALONE_WIN
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
#endif


        public void Dispose()
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
        }
    }
    
}