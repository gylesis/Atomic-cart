using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dev.Infrastructure
{
    public class GlobalDisposable
    {
        private static GameObject _disposable;
        public static GlobalDisposable Instance { get; private set; }
        
        public static CancellationToken DestroyCancellationToken => _disposable.GetCancellationTokenOnDestroy();
        
        public GlobalDisposable()
        {
            Instance = this;
            _disposable = new GameObject("[Global Disposable]");
            Object.DontDestroyOnLoad(_disposable); 
        }
    }
}