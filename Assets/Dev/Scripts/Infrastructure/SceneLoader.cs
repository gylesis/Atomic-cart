using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class SceneLoader : NetworkContext
    {
        [SerializeField] private string _sceneName;
        private NetworkRunner _networkRunner;

        [ContextMenu(nameof(LoadScene))]
        private void LoadScene()
        {
            _networkRunner = FindObjectOfType<NetworkRunner>();

            _networkRunner.SetActiveScene(_sceneName);
        }

        public void LoadScene(string sceneName)
        {
            _networkRunner = FindObjectOfType<NetworkRunner>();
            _networkRunner.SetActiveScene(sceneName);
        }
    }   
}