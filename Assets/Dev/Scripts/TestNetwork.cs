using System;
using Dev.Infrastructure;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev
{
    public class TestNetwork : NetworkContext
    {
        [SerializeField] [Networked] private int Num { get; set; }

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
        }

        private void OnGUI()
        {
            if(_init == false) return;
            
            if (GUI.Button(new Rect(100, 100, 100, 100) ,"Add"))
            {
                AddNum(true);
            }
            
            if (GUI.Button(new Rect(200, 100, 100, 100) ,"Remove"))
            {
                AddNum(false);
            }

            GUI.Label(new Rect(150, 200, 100, 100), $"{Num}");
        }

        private void AddNum(bool toAdd)
        {
            if (toAdd)
            {
                Num++;
            }
            else
            {
                Num--;
            }
        }
        
    }
}