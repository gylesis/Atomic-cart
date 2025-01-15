#if !UNITY_STANDALONE_WIN
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure.Networking;
using Dev.Utils;
using ModestTree;
using UniRx;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.Core;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Dev.Infrastructure
{
    public class AuthService : IInitializable, IDisposable
    {
        public string Token;
        public string Error;
        
        public bool IsAuthorized { get; private set; }
        public string PlayerId
        {
            get
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                    return "not_initialized";
                
                if(!AuthenticationService.Instance.IsAuthorized)
                    return "not_authorized";
                
                return AuthenticationService.Instance.PlayerId;
            }
        }

        public bool IsCurrentAccountLinkedToSomething =>
            AuthenticationService.Instance.PlayerInfo.Identities.Count > 0;

        public Profile MyProfile => _cachedMyProfile;
        
        private Dictionary<string, Profile> _cachedProfiles = new Dictionary<string, Profile>();
        //private Dictionary<PlayerRef, string> _playerRefMap = new Dictionary<PlayerRef, string>();
        private SaveLoadService _saveLoadService;

        private Profile _cachedMyProfile;

        public AuthService(SaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
        }

        public void Initialize()
        {
            _saveLoadService.ProfileChanged.Subscribe(OnProfileSaveOrLoad).AddTo(GlobalDisposable.ProjectScopeToken);
        }

        private void OnProfileSaveOrLoad(Profile profile)
        {
            _cachedProfiles[PlayerId] = profile;
            _cachedMyProfile = profile;
        }

        public async UniTask<Result> Auth()
        {   
            //await LoginGooglePlayGames();
            //await SignInWithGooglePlayGamesAsync(Token);
            
            AuthenticationService.Instance.SignedIn += OnSignedIn; 
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;
            
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                IsAuthorized = false;
                AtomicLogger.Ex(e.Message);
                return Result.Error(e.Message);
            }
            
            IsAuthorized = true;
            return Result.Success();
        }

        private async UniTask<bool> SignInWithGooglePlayGamesAsync(string authCode)
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

        public async UniTask<Result<Profile>> GetMyProfileAsync(bool requestFreshData = false)
        {
            var tryGetProfile = await GetProfileAsync(PlayerId, requestFreshData);
            
            if(tryGetProfile.IsSuccess)
                _cachedMyProfile = tryGetProfile.Data;
            
            return tryGetProfile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="requestFreshData">Request fresh data from Cloud. It needs when you know that old data can be changed. </param>
        /// <returns></returns>
        public async UniTask<Result<Profile>> GetProfileAsync(string playerId, bool requestFreshData = false)
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                return Result<Profile>.Error($"Services not initialized.");

            if (requestFreshData == false)
            {
                if (_cachedProfiles.TryGetValue(playerId, out Profile profile))
                    return Result<Profile>.Success(profile);
                
                AtomicLogger.Log($"No profile found for player {playerId}, requesting fresh one", AtomicConstants.LogTags.Networking);
            }
            
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{"profile"}, new LoadOptions(new PublicReadAccessClassOptions(playerId)));

            bool tryGetValue = playerData.TryGetValue("profile", out Item profileItem);

            if (tryGetValue)
            {       
                var asString = profileItem.Value.GetAsString();
                Profile profile = JsonUtility.FromJson<Profile>(asString);
                _cachedProfiles[playerId] = profile;
                return Result<Profile>.Success(profile);
            }

            return Result<Profile>.Error($"No profile found for ID: {playerId}.");
        }

        public bool TryGetCachedProfile(string playerId, out Profile profile)
        {
            return _cachedProfiles.TryGetValue(playerId, out profile);
        }

        public async UniTask<Result<string>> GetNicknameAsync(string playerId, bool requestFreshData = false)
        {
            var tryGetProfile = await GetProfileAsync(playerId, requestFreshData);
            
            if(tryGetProfile.IsError)
                return Result<string>.Error(tryGetProfile.ErrorMessage);

            Profile profile = tryGetProfile.Data;

            return Result<string>.Success(profile.Nickname);
        }

        public async UniTask DeleteAccountAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                AtomicLogger.Log($"Services not initialized. Deleting is failed", AtomicConstants.LogTags.Networking);
                return;
            }
            
            await AuthenticationService.Instance.DeleteAccountAsync();
        }

        public async UniTask<Result> LinkWithUsernameAndPasswordAsync(string username, string password)
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                AtomicLogger.Log($"Services not initialized. Linking is failed", AtomicConstants.LogTags.Networking);
                return Result.Error($"Services not initialized. Linking is failed.");
            }
            
            try
            {
                await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
            }
            catch (Exception e)
            {
                AtomicLogger.Ex(e.Message, AtomicConstants.LogTags.Networking);
                return Result.Error(e.Message);
            }
            
            return Result.Success();
        }

        private void OnSignedIn()
        {
            AtomicLogger.Log($"Signed In", AtomicConstants.LogTags.Networking);
        }

        private void OnSignInFailed(RequestFailedException exception)
        {
            AtomicLogger.Err($"Sign in Failed: {exception.Message}", AtomicConstants.LogTags.Networking);
        }

        public void Dispose()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                AuthenticationService.Instance.SignedIn -= OnSignedIn;
                AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
            }
        }
    }
}