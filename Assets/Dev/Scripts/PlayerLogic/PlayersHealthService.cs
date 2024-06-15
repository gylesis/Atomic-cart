using System;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersHealthService : NetworkContext
    {
        private PlayersSpawner _playersSpawner;

        [Networked, Capacity(20)] private NetworkDictionary<PlayerRef, int> PlayersHealth { get; }

        public Subject<PlayerDieEventContext> PlayerKilled { get; } = new Subject<PlayerDieEventContext>();

        private bool _init;
        private TeamsService _teamsService;
        private WorldTextProvider _worldTextProvider;
        private CharactersDataContainer _charactersDataContainer;
        private GameSettings _gameSettings;
        private HealthObjectsService _healthObjectsService;

        [Inject]
        public void Init(PlayersSpawner playersSpawner, TeamsService teamsService, WorldTextProvider worldTextProvider,
                         GameStaticDataContainer gameStaticDataContainer, GameSettings gameSettings,
                         HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
            _gameSettings = gameSettings;
            _charactersDataContainer = gameStaticDataContainer.CharactersDataContainer;
            _playersSpawner = playersSpawner;
            _teamsService = teamsService;
            _worldTextProvider = worldTextProvider;
        }

        public override void Spawned()
        {
            _init = true;

            _playersSpawner.PlayerBaseSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerBaseSpawned));
            _playersSpawner.PlayerBaseDeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerBaseDespawned));
        }

        private void OnPlayerBaseSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            PlayerCharacter playerCharacter = _playersSpawner.GetPlayer(spawnEventContext.PlayerRef);

            int startHealth = _charactersDataContainer.GetCharacterDataByClass(spawnEventContext.CharacterClass)
                .CharacterStats.Health;

            _healthObjectsService.RegisterObject(playerCharacter.Object, startHealth);

            PlayerRef playerRef = spawnEventContext.PlayerRef;

            //
            if (PlayersHealth.ContainsKey(playerRef)) return;

            PlayersHealth.Add(playerRef, startHealth);
        }

        private void OnPlayerBaseDespawned(PlayerRef playerRef)
        {
            PlayerCharacter playerCharacter = _playersSpawner.GetPlayer(playerRef);


            _healthObjectsService.UnregisterObject(playerCharacter.Object);

            //
            PlayersHealth.Remove(playerRef);
        }

        public void ApplyDamage(PlayerRef victim, PlayerRef shooter, int damage)
        {
            if (_gameSettings.IsFriendlyFireOn == false)
            {
                TeamSide victimTeamSide = _teamsService.GetUnitTeamSide(victim);
                TeamSide shooterTeamSide = _teamsService.GetUnitTeamSide(shooter);

                if (victimTeamSide == shooterTeamSide) return;
            }

            int playerCurrentHealth = PlayersHealth[victim];

            if (playerCurrentHealth == 0) return;

            var nickname = PlayersDataService.Instance.GetNickname(victim);

            Debug.Log($"Damage {damage} applied to player {nickname}");
            //LoggerUI.Instance.Log($"Damage {damage} applied to player {nickname}");

            Vector3 playerPos = _playersSpawner.GetPlayerPos(victim);
            RPC_SpawnDamageHintFor(shooter, playerPos, damage);
            RPC_SpawnDamageHintFor(victim, playerPos, damage);

            playerCurrentHealth -= damage;

            if (playerCurrentHealth <= 0)
            {
                playerCurrentHealth = 0;
                OnPlayerHealthZero(victim, shooter);
            }

            Debug.Log($"Player {nickname} has {playerCurrentHealth} health");

            RPC_UpdatePlayerHealth(victim, playerCurrentHealth);
        }

        public void ApplyDamageFromServer(PlayerRef victim, int damage)
        {
            //if (HasStateAuthority == false) return;

            int playerCurrentHealth = PlayersHealth[victim];

            if (playerCurrentHealth == 0) return;

            var nickname = PlayersDataService.Instance.GetNickname(victim);

            Debug.Log($"Damage {damage} applied to player {nickname} from server");

            Vector3 playerPos = _playersSpawner.GetPlayerPos(victim);
            RPC_SpawnDamageHintFor(victim, playerPos, damage);

            playerCurrentHealth -= damage;

            if (playerCurrentHealth <= 0)
            {
                playerCurrentHealth = 0;
                OnPlayerHealthZero(victim, PlayerRef.None);
            }

            Debug.Log($"Player {nickname} has {playerCurrentHealth} health");

            RPC_UpdatePlayerHealth(victim, playerCurrentHealth);
        }

        public void ApplyDamageToDummyTarget(DummyTarget dummyTarget, PlayerRef shooter, int damage)
        {
            Debug.Log($"Damage {damage} applied to dummy target {dummyTarget.name}");

            Vector3 playerPos = dummyTarget.transform.position;
            RPC_SpawnDamageHintFor(shooter, playerPos, damage);
        }

        private void OnPlayerHealthZero(PlayerRef playerRef, PlayerRef owner)
        {
            PlayerCharacter playerCharacter = _playersSpawner.GetPlayer(playerRef);
            PlayerBase playerBase = _playersSpawner.GetPlayerBase(playerRef);
    
            playerCharacter.RPC_OnDeath();

            playerBase.PlayerController.SetAllowToMove(false);
            playerBase.PlayerController.SetAllowToShoot(false);

            var playerDieEventContext = new PlayerDieEventContext();
            playerDieEventContext.Killer = owner;
            playerDieEventContext.Killed = playerRef;

            PlayerKilled.OnNext(playerDieEventContext);

            //LoggerUI.Instance.Log($"Player {playerRef} is dead");

            float respawnTime = 2;

            Observable.Timer(TimeSpan.FromSeconds(respawnTime)).Subscribe((l =>
            {
                _playersSpawner.RespawnPlayerCharacter(playerRef);
            }));
        }

        public void RestorePlayerHealth(PlayerRef playerRef)
        {
            PlayerBase playerBase = _playersSpawner.GetPlayerBase(playerRef);

            CharacterData characterData =
                _charactersDataContainer.GetCharacterDataByClass(playerBase.CharacterClass);

            GainHealthToPlayer(playerRef, characterData.CharacterStats.Health);
        }

        public void GainHealthToPlayer(PlayerRef playerRef, int health)
        {
            Debug.Log($"Gained {health} HP for player {playerRef}");

            PlayerBase playerBase = _playersSpawner.GetPlayerBase(playerRef);

            CharacterData characterData =
                _charactersDataContainer.GetCharacterDataByClass(playerBase.CharacterClass);

            int maxHealth = characterData.CharacterStats.Health;

            int currentHealth = PlayersHealth[playerRef];
            currentHealth += health;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            RPC_UpdatePlayerHealth(playerRef, currentHealth);
        }

        [Rpc]
        private void RPC_SpawnDamageHintFor([RpcTarget] PlayerRef playerRef, Vector3 pos, int damage)
        {
            _worldTextProvider.SpawnDamageText(pos, damage);
        }

        [Rpc]
        private void RPC_UpdatePlayerHealth(PlayerRef playerRef, int health)
        {
            PlayersHealth.Set(playerRef, health);
        }

        private void OnGUI()
        {
            if (_init == false) return;

            float height = 50;

            foreach (var pair in PlayersHealth)
            {
                var rect = new Rect(50, height, 100, 20);

                PlayerRef playerRef = pair.Key;
                
                if(PlayersDataService.Instance.HasData(playerRef) == false) continue;
                
                string nickname = PlayersDataService.Instance.GetNickname(playerRef);
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
    }

    public struct PlayerDieEventContext
    {
        public PlayerRef Killer;
        public PlayerRef Killed;
    }
}