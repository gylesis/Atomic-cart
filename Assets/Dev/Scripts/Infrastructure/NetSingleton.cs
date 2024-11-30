using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Utils;

namespace Dev
{
    public abstract class NetSingleton<T> : NetworkContext where T : NetworkContext
    {
        public static T Instance { get; private set; }

        public static bool IsNetInitialized { get; private set; }
        public static bool IsInitialized { get; private set; }

        protected override void Awake()
        {
            if (Instance != null) 
                AtomicLogger.Err($"Multiple singleton for [{typeof(T)}]");
            
            Instance = this as T;
            IsInitialized = true;
            
            base.Awake();
        }

        public override void Spawned()
        {
            base.Spawned();
            IsNetInitialized = true;
        }

        public static async UniTask<T> WaitForNetInitialization()
        {
            await UniTask.WaitUntil(() => IsNetInitialized, cancellationToken: GlobalDisposable.DestroyCancellationToken);
            return Instance;
        }
    }
}