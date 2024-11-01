using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Utils;
using Newtonsoft.Json;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using Random = UnityEngine.Random;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;

namespace Dev
{
    public class SaveLoadService
    {
        private ISaveLoadScheme _saveLoadScheme;
        
        public Profile Profile { get; private set; }

        public static SaveLoadService Instance { get; private set; }
        
        public SaveLoadService(ISaveLoadScheme saveLoadScheme)
        {
            _saveLoadScheme = saveLoadScheme;
            Instance = this;
        }
        
        public async UniTask Load()
        {
            Profile = await _saveLoadScheme.Load();
        }

        public async UniTask Save(Action<Profile> changedCallback)
        {   
            changedCallback?.Invoke(Profile);
            await _saveLoadScheme.Save(Profile);
        }

        public void AddKill(SessionPlayer sessionPlayer)
        {
            if(sessionPlayer.IsBot) return;

            Save(profile => profile.Kills++).Forget();
        }
        
        public void AddDeath(SessionPlayer sessionPlayer)
        {
            if(sessionPlayer.IsBot) return;

            Save(profile => profile.Deaths++).Forget();
        }
        
        public interface ISaveLoadScheme
        {
            UniTask<bool> Save(Profile profile);
            UniTask<Profile> Load();
        }
        
        public class LocalSaveLoadScheme : ISaveLoadScheme
        {
            private const string PlayerSaveKey = "PlayerSave";
        
            public UniTask<bool> Save(Profile profile)
            {
                string json = JsonConvert.SerializeObject(profile);
                PlayerPrefs.SetString(PlayerSaveKey, json);
                PlayerPrefs.Save();
                return UniTask.FromResult(true);
            }

            public UniTask<Profile> Load()
            {
                string json = PlayerPrefs.GetString(PlayerSaveKey);

                Profile profile;
                if (string.IsNullOrEmpty(json))
                {
                    profile = new Profile();
                    profile.Nickname = $"Player{Random.Range(1, 100)}";
                }
                else
                    profile = JsonConvert.DeserializeObject<Profile>(json);

                return UniTask.FromResult(profile);
            }
        }

        public class CloudSaveLoadScheme : ISaveLoadScheme
        {
            private HashSet<string> _fieldNames = new HashSet<string>() { "profile"};
            
            private static string PlayerSaveKey => AtomicConstants.SaveLoad.PlayerSaveKey;

        
            public async UniTask<bool> Save(Profile profile)
            {
                string serializeObject = JsonConvert.SerializeObject(profile);
            
                var data = new Dictionary<string, object> { {"profile", serializeObject} };

                await CloudSaveService.Instance.Data.Player.SaveAsync(data, new SaveOptions(new PublicWriteAccessClassOptions()));
                
                return true;
            }

            public async UniTask<Profile> Load()
            {
                List<FileItem> fileItems = await CloudSaveService.Instance.Files.Player.ListAllAsync();

                Profile profile = new Profile();

                bool isNewPlayer = fileItems.Count == 0;
                if (isNewPlayer)
                {
                    await Save(profile);
                    return profile;
                }

                Dictionary<string,Item> profileItems = await CloudSaveService.Instance.Data.Player.LoadAsync(_fieldNames, new LoadOptions(new PublicReadAccessClassOptions()));

                var tryGetValue = profileItems.TryGetValue("profile", out var profileItem);
                if (tryGetValue)
                {
                    var asString = profileItem.Value.GetAsString();
                    profile = JsonUtility.FromJson<Profile>(asString);
                }

                AtomicLogger.Log($"Profile loaded");
                return profile;
            }
        }
        
    }

    
}