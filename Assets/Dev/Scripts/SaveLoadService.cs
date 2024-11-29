using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Utils;
using Newtonsoft.Json;
using UniRx;
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
        public Profile Profile { get; private set; }

        private Dictionary<Type, ISaveLoadScheme> _loadSchemes = new Dictionary<Type, ISaveLoadScheme>();

        public static SaveLoadService Instance { get; private set; }

        public Subject<Profile> ProfileChanged { get; } = new Subject<Profile>();

        public SaveLoadService(ISaveLoadScheme[] saveLoadSchemes)
        {
            foreach (var saveLoadScheme in saveLoadSchemes)
                _loadSchemes[saveLoadScheme.GetType()] = saveLoadScheme;

            Instance = this;
        }

        public async UniTask Load()
        {
            Profile = await GetSaveLoadScheme().Load();
            ProfileChanged.OnNext(Profile);
        }

        public async UniTask<Result> Save(Action<Profile> changedCallback)
        {
            changedCallback?.Invoke(Profile);
            var result = await GetSaveLoadScheme().Save(Profile);

            if (result.IsSuccess)
                ProfileChanged.OnNext(Profile);

            return result;
        }

        public void AddKill(SessionPlayer sessionPlayer)
        {
            if (sessionPlayer.IsBot) return;

            Save(profile => profile.Kills++).Forget();
        }

        public void AddDeath(SessionPlayer sessionPlayer)
        {
            if (sessionPlayer.IsBot) return;

            Save(profile => profile.Deaths++).Forget();
        }

        private ISaveLoadScheme GetSaveLoadScheme() =>
            _loadSchemes[LobbyConnector.IsInitialized ? typeof(CloudSaveLoadScheme) : typeof(LocalSaveLoadScheme)];

        public interface ISaveLoadScheme
        {
            UniTask<Result> Save(Profile profile);
            UniTask<Profile> Load();
        }

        public class LocalSaveLoadScheme : ISaveLoadScheme
        {
            private const string PlayerSaveKey = "PlayerSave";

            public UniTask<Result> Save(Profile profile)
            {
                string json = JsonConvert.SerializeObject(profile);
                PlayerPrefs.SetString(PlayerSaveKey, json);
                PlayerPrefs.Save();
                return UniTask.FromResult(Result.Success());
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
            private HashSet<string> _fieldNames = new HashSet<string>() { "profile" };

            private static string PlayerSaveKey => AtomicConstants.SaveLoad.PlayerSaveKey;


            public async UniTask<Result> Save(Profile profile)
            {
                string serializeObject = JsonConvert.SerializeObject(profile);

                var data = new Dictionary<string, object> { { "profile", serializeObject } };

                try
                {
                    await CloudSaveService.Instance.Data.Player.SaveAsync(data,
                        new SaveOptions(new PublicWriteAccessClassOptions()));
                }
                catch (Exception e)
                {
                    AtomicLogger.Err(e.Message, AtomicConstants.LogTags.Networking);
                    return Result.Error(GameSettingsProvider.GameSettings.IsDebugMode
                        ? $"{e.Message}"
                        : "Failed to save player");
                }

                return Result.Success();
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

                Dictionary<string, Item> profileItems =
                    await CloudSaveService.Instance.Data.Player.LoadAsync(_fieldNames,
                        new LoadOptions(new PublicReadAccessClassOptions()));

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