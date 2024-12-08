using Cysharp.Threading.Tasks;

namespace Dev.Infrastructure.Networking
{
    public interface IInitializableModule
    {
        bool IsInitialized { get; }
        UniTask<Result> Initialize();
    }
}