using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev.Infrastructure
{
    [RequireComponent(typeof(NetworkRunner))]
    public class FusionInitializer : MonoBehaviour
    {
        private NetworkRunner _networkRunner;

        private void Awake()
        {
            _networkRunner = GetComponent<NetworkRunner>();
            _networkRunner.AddCallbacks(FindObjectOfType<EntryPoint>());
            _networkRunner.ProvideInput = true;

            var startGameArgs = new StartGameArgs();

            startGameArgs.GameMode = GameMode.AutoHostOrClient;
            startGameArgs.SessionName = "Test";
            startGameArgs.Scene = SceneManager.GetActiveScene().buildIndex;
            startGameArgs.SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            _networkRunner.StartGame(startGameArgs);
        }
    }
}