using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Utils;

namespace Dev
{
    public abstract class NetSingleton<T> : NetworkContext where T : NetworkContext
    {
        public static T Instance { get; private set; }

        public static bool IsInitialized { get; private set; }
        
        public override void Spawned()
        {
            if (Instance != null) 
                AtomicLogger.Err($"Multiple singleton for [{typeof(T)}]");
            
            Instance = this as T;
            IsInitialized = true;
            
            base.Spawned();
        }

        public static async UniTask<T> WaitForInitialization()
        {
            await UniTask.WaitUntil(() => IsInitialized, cancellationToken: GlobalDisposable.DestroyCancellationToken);
            return Instance;
        }
    }
}