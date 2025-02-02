﻿using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Dev.Infrastructure
{
    public class GlobalDisposable
    {
        private static GameObject _projectDisposable;
        private static GameObject _sceneDisposable;
        public static GlobalDisposable Instance { get; private set; }
        
        public static CancellationToken ProjectScopeToken => _projectDisposable.GetCancellationTokenOnDestroy();
        public static CancellationToken SceneScopeToken => _sceneDisposable.GetCancellationTokenOnDestroy();
        
        public GlobalDisposable()
        {
            Instance = this;
            _projectDisposable = new GameObject("[Global Disposable]");
            Object.DontDestroyOnLoad(_projectDisposable); 
            
            UpdateSceneDisposable();
            
            SceneManager.sceneUnloaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene)
        {
            if(scene.name == "Bootstrap" || scene.name == "AuthScene")
                return;
            
            UpdateSceneDisposable();
        }

        private static void UpdateSceneDisposable()
        {
            if(_sceneDisposable != null)
                Object.Destroy(_sceneDisposable);
            
            _sceneDisposable = new GameObject("[Scene Disposable]");
            Object.DontDestroyOnLoad(_sceneDisposable); 
        }
    }
}