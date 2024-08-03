using Cysharp.Threading.Tasks;
using Fusion;
using UniRx;
using UnityEngine.SceneManagement;

namespace Dev.Infrastructure
{
    public class SceneLoader
    {
        public Subject<string> SceneLoaded { get; private set; } = new Subject<string>();

        public void LoadSceneNet(NetworkRunner runner, SceneRef sceneRef)
        {
            runner.LoadScene(sceneRef);
        }

        public async UniTask LoadSceneLocal(string sceneName, LoadSceneMode loadSceneMode)
        {
            await SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            SceneLoaded.OnNext(sceneName);
        }

        public async UniTask UnloadSceneLocal(string sceneName)
        {
            await SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}   