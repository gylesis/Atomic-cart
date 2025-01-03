﻿using System;
using Cysharp.Threading.Tasks;
using Dev.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using LogType = Fusion.LogType;

namespace Dev.Infrastructure
{
    public class Bootstrap : MonoBehaviour
    {
        private async void Awake()
        {
            Curtains curtains = await Curtains.WaitForInitialization();
            LoadAuthScene();
        }

        private async void LoadAuthScene()
        {
            float showDuration = 0.1f;

            Fusion.Log.LogLevel = LogType.Error;
            
            Curtains.Instance.SetText("Loading");
            Curtains.Instance.Show(showDuration);
            
            await UniTask.Delay(TimeSpan.FromSeconds(showDuration));

            SceneManager.LoadSceneAsync("AuthScene");
        }
    }
}