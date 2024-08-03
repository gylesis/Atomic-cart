using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev
{
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            LoadAuthScene();
        }

        private async void LoadAuthScene()
        {
            float showDuration = 0.1f;
            
            Curtains.Instance.SetText("Loading");
            Curtains.Instance.Show(showDuration);
            
            await UniTask.Delay(TimeSpan.FromSeconds(showDuration));

            SceneManager.LoadSceneAsync("AuthScene");
        }
    }
}