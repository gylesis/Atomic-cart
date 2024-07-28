using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Utils;
using Newtonsoft.Json;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;

namespace Dev
{
    public class SaveLoadService
    {
        private static string PlayerSaveKey => AtomicConstants.SaveLoad.PlayerSaveKey;
        
        public Profile Profile { get; private set; }

        public static SaveLoadService Instance { get; private set; }
        
        public SaveLoadService()
        {
            Instance = this;
        }
        
        public async Task Load()
        {
            List<FileItem> fileItems = await CloudSaveService.Instance.Files.Player.ListAllAsync();

            if (fileItems.Count == 0)
            {
                Profile = new Profile();
                Profile.Nickname = AuthService.Nickname;
                
                return;
            }
            
            var task = CloudSaveService.Instance.Files.Player.LoadBytesAsync(PlayerSaveKey);
            await task;

            AtomicLogger.Log($"Profile loaded");
            
            byte[] bytes = task.Result;
            string json = Encoding.Default.GetString(bytes);

            Profile = JsonConvert.DeserializeObject<Profile>(json);
        }

        public async Task Save(Action<Profile> changedCallback)
        {   
            changedCallback?.Invoke(Profile);
            
            string serializeObject = JsonConvert.SerializeObject(Profile);
            byte[] bytes = Encoding.Default.GetBytes(serializeObject);

            Task saveTask = CloudSaveService.Instance.Files.Player.SaveAsync(PlayerSaveKey, bytes);
            await saveTask;
            
            AtomicLogger.Log($"Profile saved");
        }

        public void AddKill(SessionPlayer sessionPlayer)
        {
            if(sessionPlayer.IsBot) return;

            Save((profile => profile.Kills++));
        }
        
        public void AddDeath(SessionPlayer sessionPlayer)
        {
            if(sessionPlayer.IsBot) return;

            Save((profile => profile.Deaths++));
        }
        
    }
}