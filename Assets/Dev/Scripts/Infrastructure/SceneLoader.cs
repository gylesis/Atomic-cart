using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure
{
    public class SceneLoader : NetworkSceneManagerBase
    {
        [SerializeField] private string _sceneName = "Main";
        
        private NetworkRunner _networkRunner;
        private Scene _loadedScene;
        private NetworkRunner _runner;

        private void Awake()
        {
            _loadedScene = SceneManager.GetActiveScene();
        }

        [Inject]
        private void Init(NetworkRunner runner)
        {
            _networkRunner = runner;
        }

        [ContextMenu(nameof(LoadScene))]
        private void LoadScene()
        {
            _networkRunner.SetActiveScene(_sceneName);
        }

        public void LoadScene(string sceneName)
        {
            if(SceneManager.GetActiveScene().name == sceneName) return;
            
            _networkRunner.SetActiveScene(sceneName);
        }
        
        protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene,
            FinishedLoadingDelegate finished)
        {
//             Debug.Log($"Switching Scene from {prevScene} to {newScene}");

            if (newScene <= 0)
            {
                finished(FindNetworkObjects(SceneManager.GetActiveScene()));
                yield break;
            }

            if (prevScene > 0)
            {
                // yield return new WaitForSeconds(1.0f);

                // Debug.Log("De-spawning all players");

                /*foreach (Player player in _playersSpawner.Players)
                {
                    PlayerRef playerRef = player.PlayerRef;
                    
                    //Debug.Log($"De-spawning player {_playersDataService.GetNickname(playerRef)}");
                    Debug.Log($"De-spawning player {playerRef}");
                    
                    _playersSpawner.SetPlayerActiveState(playerRef, false);
                    
                    yield return new WaitForSeconds(0.1f);
                }*/

                // yield return new WaitForSeconds(1.5f - PlayerManager.allPlayers.Count * 0.1f);

                //Debug.Log("De-spawned all players");
            }

            yield return null;
            //Debug.Log($"Start loading scene {newScene} in single peer mode");

            _loadedScene = default;
            //Debug.Log($"Loading scene {newScene}");

            List<NetworkObject> sceneObjects = new List<NetworkObject>();

            if (newScene >= 0)
            {
                yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
                _loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
               // Debug.Log($"Loaded scene {newScene}: {_loadedScene}");
                sceneObjects = FindNetworkObjects(_loadedScene, disable: false);
            }

            // Delay one frame
            yield return null;

            //Debug.Log($"Switched Scene from {prevScene} to {newScene} - loaded {sceneObjects.Count} scene objects");
            finished(sceneObjects);


            // Debug.Log($"Unloading Scene {0}");
            SceneManager.UnloadSceneAsync(0);
        }
    }
}