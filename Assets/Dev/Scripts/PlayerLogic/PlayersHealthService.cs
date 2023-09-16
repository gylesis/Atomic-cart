using System;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersHealthService : NetworkContext
    {
        private PlayersSpawner _playersSpawner;

        [Networked, Capacity(20)] private NetworkDictionary<PlayerRef, int> PlayersHealth { get; }

        public static PlayersHealthService Instance { get; private set; }
        public Subject<PlayerDieEventContext> PlayerKilled { get; } = new Subject<PlayerDieEventContext>();

        private bool _init;
        private TeamsService _teamsService;
        private WorldTextProvider _worldTextProvider;
        private CharactersDataContainer _charactersDataContainer;
        private GameSettings _gameSettings;

        private void OnGUI()
        {
            if (_init == false) return;

            float height = 0;

            foreach (var pair in PlayersHealth)
            {
                var rect = new Rect(0, height, 100, 20);

                string nickname = PlayersDataService.Instance.GetNickname(pair.Key);
                int health = pair.Value;

                var guiStyle = new GUIStyle();
                guiStyle.fontSize = 40;

                Color color = Color.white;

                if (health == 0)
                {
                    color = Color.red;
                }

                guiStyle.normal.textColor = color;

                GUI.Label(rect, $"{nickname}: {health} HP", guiStyle);

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

        [Inject]
        public void Init(PlayersSpawner playersSpawner, TeamsService teamsService, WorldTextProvider worldTextProvider,
            CharactersDataContainer charactersDataContainer, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _charactersDataContainer = charactersDataContainer;
            _playersSpawner = playersSpawner;
            _teamsService = teamsService;
            _worldTextProvider = worldTextProvider;
        }

        public override void Spawned()
        {
            _init = true;

            //if (Runner.IsSharedModeMasterClient == false) return;

            _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            _playersSpawner.DeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            PlayerRef playerRef = spawnEventContext.PlayerRef;

            if(PlayersHealth.ContainsKey(playerRef)) return;
            
            int startHealth = _charactersDataContainer.GetCharacterDataByClass(spawnEventContext.CharacterClass)
                .CharacterStats.Health;

            PlayersHealth.Add(playerRef, startHealth);
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            PlayersHealth.Remove(playerRef);
        }

        public void ApplyDamage(PlayerRef victim, PlayerRef shooter, int damage)
        {
            //if (Runner.IsSharedModeMasterClient == false) return;

            if (_gameSettings.IsFriendlyFireOn == false)
            {
                TeamSide victimTeamSide = _teamsService.GetPlayerTeamSide(victim);
                TeamSide shooterTeamSide = _teamsService.GetPlayerTeamSide(shooter);

                if (victimTeamSide == shooterTeamSide) return;
            }

            int playerCurrentHealth = PlayersHealth[victim];

            if (playerCurrentHealth == 0) return;

            var nickname = PlayersDataService.Instance.GetNickname(victim);

            Debug.Log($"Damage {damage} applied to player {nickname}");

            Vector3 playerPos = _playersSpawner.GetPlayerPos(victim);
            RPC_SpawnDamageHint(shooter, playerPos, damage);
            RPC_SpawnDamageHint(victim, playerPos, damage);

            playerCurrentHealth -= damage;

            if (playerCurrentHealth <= 0)
            {
                playerCurrentHealth = 0;
                OnPlayerHealthZero(victim, shooter);
            }

            Debug.Log($"Player {nickname} has {playerCurrentHealth} health");

            PlayersHealth.Set(victim, playerCurrentHealth);
        }
        
        
        public void ApplyDamageFromServer(PlayerRef victim, int damage)
        {   
            //if (HasStateAuthority == false) return;

            int playerCurrentHealth = PlayersHealth[victim];

            if (playerCurrentHealth == 0) return;

            var nickname = PlayersDataService.Instance.GetNickname(victim);

            Debug.Log($"Damage {damage} applied to player {nickname} from server");

            Vector3 playerPos = _playersSpawner.GetPlayerPos(victim);
            RPC_SpawnDamageHint(victim, playerPos, damage);

            playerCurrentHealth -= damage;

            if (playerCurrentHealth <= 0)
            {
                playerCurrentHealth = 0;
                OnPlayerHealthZero(victim, PlayerRef.None);
            }

            Debug.Log($"Player {nickname} has {playerCurrentHealth} health");

            PlayersHealth.Set(victim, playerCurrentHealth);
        }
        

        public void ApplyDamageToDummyTarget(DummyTarget dummyTarget, PlayerRef shooter, int damage)
        {
            Debug.Log($"Damage {damage} applied to dummy target {dummyTarget.name}");

            Vector3 playerPos = dummyTarget.transform.position;
            RPC_SpawnDamageHint(shooter, playerPos, damage);
        }

        [Rpc]
        private void RPC_SpawnDamageHint([RpcTarget] PlayerRef playerRef, Vector3 pos, int damage)
        {
            _worldTextProvider.SpawnDamageText(pos, damage);
        }

        private void OnPlayerHealthZero(PlayerRef playerRef, PlayerRef owner)
        {
            Player player = _playersSpawner.GetPlayer(playerRef);
            player.RPC_DoScale(0.5f, 0f);

            player.PlayerController.AllowToMove = false;
            player.PlayerController.AllowToShoot = false;
            player.HitboxRoot.HitboxRootActive = false;

            var playerDieEventContext = new PlayerDieEventContext();
            playerDieEventContext.Killer = owner;
            playerDieEventContext.Killed = playerRef;

            PlayerKilled.OnNext(playerDieEventContext);

            Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe((l =>
            {
                _playersSpawner.RespawnPlayer(playerRef);

                player.RPC_DoScale(0);
            }));
        }

        public void RestorePlayerHealth(PlayerRef playerRef)
        {
            Player player = _playersSpawner.GetPlayer(playerRef);

            CharacterData characterData = _charactersDataContainer.GetCharacterDataByClass(player.CharacterClass);

            Debug.Log($"Restoring health for player {playerRef} - {characterData.CharacterStats.Health}");

            PlayersHealth.Set(playerRef, characterData.CharacterStats.Health);
        }
    }

    public struct PlayerDieEventContext
    {
        public PlayerRef Killer;
        public PlayerRef Killed;
    }
}