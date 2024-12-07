using Cysharp.Threading.Tasks;
using Dev.Utils;
using UnityEngine;

namespace Dev.Infrastructure
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        public static bool IsInitialized { get; private set; }

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
            return Instance;
        }

        protected virtual void OnDestroy()
        {
            Instance = null;
            IsInitialized = false;
        }
    }
}