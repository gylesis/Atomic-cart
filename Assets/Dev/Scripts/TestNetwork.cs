using System.Linq;
using Dev.Infrastructure;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Dev
{
    public class TestNetwork : NetworkContext
    {
        [Networked, Capacity(999)] private NetworkLinkedList<int> Numbers { get; }
        
        private bool _init;

        private void Awake()
        {
            var startGameArgs = new StartGameArgs();
            startGameArgs.GameMode = GameMode.Shared;
            startGameArgs.Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            startGameArgs.SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            
            FindObjectOfType<NetworkRunner>().StartGame(startGameArgs);
        }

        public override void Spawned()
        {
            _init = true;
            Object.ReleaseStateAuthority();
        }

        private void OnGUI()
        {
            if(_init == false) return;
            
            GUI.Label(new Rect(300, 100, 100, 100), HasStateAuthority ? "Is owner" : "Not owner");
            GUI.Label(new Rect(300, 200, 100, 100), Runner.IsSharedModeMasterClient ? "Is master client" : "Isn't master client");
            
            
            if (GUI.Button(new Rect(100, 100, 100, 100) ,"Add"))
            {
                RPC_AddNum(true);
            }
            
            if (GUI.Button(new Rect(200, 100, 100, 100) ,"Remove"))
            {
                RPC_AddNum(false);
            }

            for (int i = 0; i < Numbers.Count; i++)
            {
                GUI.Label(new Rect(150, 200 + i * 10, 100, 100), $"{i}");
            }
           // GUI.Label(new Rect(150, 200, 100, 100), $"{Num}");
        }

        [Rpc]
        private void RPC_AddNum(bool toAdd)
        {
            if (toAdd)
            {
                Numbers.Add(Random.Range(0,5));
            }
            else
            {
                Numbers.Remove(Numbers.Last());
            }
        }
        
    }
}