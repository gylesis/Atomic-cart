using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev.Infrastructure
{
    public class SceneLoader : NetworkSceneManagerDefault
    {
        [SerializeField] private string _sceneName = "Main";

        private NetworkRunner _networkRunner => FindObjectOfType<NetworkRunner>();
        private Scene _loadedScene;

        private void Awake()
        {
            _loadedScene = SceneManager.GetActiveScene();
        }

        [ContextMenu(nameof(LoadScene))]
        private void LoadScene()
        {
            _networkRunner.LoadScene(_sceneName);
        }

        public void LoadScene(string sceneName)
        {
            if(SceneManager.GetActiveScene().name == sceneName) return;
            
            _networkRunner.LoadScene(sceneName, setActiveOnLoad: true);
        }
    }
}