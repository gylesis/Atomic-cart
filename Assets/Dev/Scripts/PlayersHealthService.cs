using System;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev
{
    public class PlayersHealthService : NetworkContext
    {
        private PlayersSpawner _playersSpawner;

        [Networked]
        private NetworkDictionary<PlayerRef, int> PlayersHealth { get; }

        public static PlayersHealthService Instance { get; private set; }

        private bool _init;
        
        private void OnGUI()
        {
            if(_init == false) return;
            
            float height = 0;
            
            foreach (var pair in PlayersHealth)
            {
                var rect = new Rect(0,height,100, 20);

                string nickname = PlayersDataService.Instance.GetNickname(pair.Key);
                int health = pair.Value;

                var guiStyle = new GUIStyle();
                guiStyle.fontSize = 40;
                
                Color color = Color.black;

                if (health == 0)
                {
                    color = Color.red;
                }
                
                guiStyle.normal.textColor = color;
                
                GUI.Label(rect, $"{nickname}: {health} HP" , guiStyle);
                
                height += 55;
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void Init(PlayersSpawner playersSpawner)
        {
            _playersSpawner = playersSpawner;
        }

        public override void Spawned()
        {
            _init = true;

            if(HasStateAuthority == false) return;
            
            _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            PlayerRef playerRef = spawnEventContext.PlayerRef;

            int startHealth = 100;
            PlayersHealth.Add(playerRef, startHealth);
        }

        public void ApplyDamage(PlayerRef playerRef, int damage)
        {
            if (HasStateAuthority == false) return;

            int playerCurrentHealth = PlayersHealth[playerRef];

            if(playerCurrentHealth == 0) return;
            
            var nickname = PlayersDataService.Instance.GetNickname(playerRef);
            
            Debug.Log($"Damage applied to player {nickname} with {damage} damage");
            
            playerCurrentHealth -= damage;

            if (playerCurrentHealth <= 0)
            {
                playerCurrentHealth = 0;
                OnPlayerHealthZero(playerRef);
            }

            ApplyForceToPlayer(playerRef, damage);
            
            Debug.Log($"Player {nickname} has {playerCurrentHealth} health");

            PlayersHealth.Set(playerRef, playerCurrentHealth);
        }

        private void OnPlayerHealthZero(PlayerRef playerRef)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(playerRef);

            Player player = playerObject.GetComponent<Player>();
            player.RPC_DoScale(0.5f, 0f);

            Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe((l =>
            {
                _playersSpawner.RespawnPlayer(playerRef);
                
                RestorePlayerHealth(playerRef, 100);
                
                player.RPC_DoScale(0, 1);
            }));
        }
        
        private void RestorePlayerHealth(PlayerRef playerRef, int restoreHealth)
        {
            PlayersHealth.Set(playerRef, restoreHealth);
        }
        
        private void ApplyForceToPlayer(PlayerRef playerRef, float forcePower)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(playerRef);
    
            Player player = playerObject.GetComponent<Player>();
            
            var forceDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0f, 1f));
            forceDirection.Normalize();

            Debug.DrawRay(player.transform.position, forceDirection * 2, Color.blue, 5f);

            player.Rigidbody.AddForce(forceDirection * forcePower, ForceMode2D.Impulse);
        }
    }
    
}