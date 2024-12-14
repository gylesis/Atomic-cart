using System;
using Cysharp.Threading.Tasks;
using Dev.Utils;
using UnityEngine;

namespace Dev.Infrastructure
{
    public abstract class MonoSingleton<T> : MonoContext where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        public static bool IsInitialized { get; private set; }

        private static Action<T> _onInitialized;
            
        protected virtual void Awake()
        {
            if (Instance != null) 
                AtomicLogger.Err($"Multiple singleton for [{typeof(T)}]");
            
            Instance = this as T;
            IsInitialized = true;
        }

        public static async UniTask<T> WaitForInitialization()
        {
            await UniTask.WaitUntil(() => IsInitialized, cancellationToken: GlobalDisposable.ProjectScopeToken);
            _onInitialized?.Invoke(Instance);
            _onInitialized = null;
            return Instance;
        }

        public static void InvokeOnInitialized(Action<T> action)
        {
            _onInitialized += action;  
        }
        
        protected virtual void OnDestroy()
        {
            _onInitialized = null;
            Instance = null;
            IsInitialized = false;
        }
    }
}