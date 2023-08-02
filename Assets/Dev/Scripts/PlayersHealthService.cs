using System;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Dev
{
    public class PlayersHealthService : NetworkContext
    {
        private PlayersSpawner _playersSpawner;

        [Networked, Capacity(20)]
        private NetworkDictionary<PlayerRef, int> PlayersHealth { get; }

        public static PlayersHealthService Instance { get; private set; }

        [SerializeField] private bool _isFriendlyOff = true;
        
        public Subject<PlayerDieEventContext> PlayerKilled { get; } = new Subject<PlayerDieEventContext>();

        private bool _init;
        private TeamsService _teamsService;

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

            _teamsService = FindObjectOfType<TeamsService>();
        }

        public override void Spawned()
        {
            _init = true;

            if(HasStateAuthority == false) return;
            
            _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            _playersSpawner.DeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            PlayerRef playerRef = spawnEventContext.PlayerRef;

            int startHealth = 100;
            PlayersHealth.Add(playerRef, startHealth);
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            PlayersHealth.Remove(playerRef);
        }
        
        public void ApplyDamage(PlayerRef victim, PlayerRef shooter, int damage)
        {
            if (HasStateAuthority == false) return;

            if (_isFriendlyOff)
            {
                TeamSide victimTeamSide = _teamsService.GetPlayerTeamSide(victim);
                TeamSide shooterTeamSide = _teamsService.GetPlayerTeamSide(shooter);
                
                if(victimTeamSide == shooterTeamSide) return;
            }

            int playerCurrentHealth = PlayersHealth[victim];

            if(playerCurrentHealth == 0) return;
            
            var nickname = PlayersDataService.Instance.GetNickname(victim);
            
            Debug.Log($"Damage applied to player {nickname} with {damage} damage");
            
            playerCurrentHealth -= damage;

            if (playerCurrentHealth <= 0)
            {
                playerCurrentHealth = 0;
                OnPlayerHealthZero(victim, shooter);
            }

            ApplyForceToPlayer(victim, damage);
            
            Debug.Log($"Player {nickname} has {playerCurrentHealth} health");

            PlayersHealth.Set(victim, playerCurrentHealth);
        }

        private void OnPlayerHealthZero(PlayerRef playerRef, PlayerRef owner)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(playerRef);

            Player player = playerObject.GetComponent<Player>();
            player.RPC_DoScale(0.5f, 0f);
            
            player.HitboxRoot.HitboxRootActive = false;

            var playerDieEventContext = new PlayerDieEventContext();
            playerDieEventContext.Killer = owner;
            playerDieEventContext.Killed = playerRef;
            
            PlayerKilled.OnNext(playerDieEventContext);
            
            Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe((l =>
            {
                _playersSpawner.RespawnPlayer(playerRef);
                
                RestorePlayerHealth(playerRef, 100);

                player.RPC_DoScale(0, 1);
                
                player.HitboxRoot.HitboxRootActive = true;
            }));
        }
        
        public void RestorePlayerHealth(PlayerRef playerRef, int restoreHealth)
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

    public struct PlayerDieEventContext
    {
        public PlayerRef Killer;
        public PlayerRef Killed;
    }
}