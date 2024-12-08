using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure.Networking;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI.PopUpsAndMenus;
using Dev.UI.PopUpsAndMenus.Main;
using Dev.Utils;
using Dev.Weapons;
using Dev.Weapons.Commands;
using Dev.Weapons.StaticData;
using Fusion;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Zenject;

namespace Dev.Infrastructure
{
    public class PlayersSpawner : NetSingleton<PlayersSpawner>
    {
        [SerializeField] private PlayerBase _playerBasePrefab;

        public Subject<PlayerSpawnEventContext> BaseSpawned { get; } = new Subject<PlayerSpawnEventContext>();
        public Subject<PlayerSpawnEventContext> CharacterSpawned { get; } = new Subject<PlayerSpawnEventContext>();

        public Subject<PlayerRef> BaseDespawned { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> CharacterDespawned { get; } = new Subject<PlayerRef>();

        public List<PlayerCharacter> PlayersCharacters => PlayersBases.Select(x => x.Character).ToList();
        public List<PlayerBase> PlayersBases => PlayersBaseDictionary.Select(x => x.Value).ToList();
        [Networked, Capacity(10)] private NetworkDictionary<PlayerRef, PlayerBase> PlayersBaseDictionary { get; }
        
        public int PlayersCount => PlayersBaseDictionary.Count;

        private TeamsService _teamsService;
        private PopUpService _popUpService;
        private SessionStateService _sessionStateService;
        private HealthObjectsService _healthObjectsService;
        private GameSettings _gameSettings;
        private AuthService _authService;

        [Inject]
        private void Init(TeamsService teamsService, PopUpService popUpService,
                          SessionStateService sessionStateService,
                          HealthObjectsService healthObjectsService, GameSettings gameSettings, AuthService authService)
        {
            _authService = authService;
            _gameSettings = gameSettings;
            _healthObjectsService = healthObjectsService;
            _sessionStateService = sessionStateService;
            _popUpService = popUpService;
            _teamsService = teamsService;
        }

        public void AskCharacterAndSpawn(PlayerRef playerRef)
        {
            var characterChooseMenu = _popUpService.ShowPopUp<CharacterChooseMenu>();

            characterChooseMenu.StartChoosingCharacter((characterClass =>
            {
                SpawnByCharacter(characterClass, playerRef, Runner);

                Extensions.Delay(0.5f, destroyCancellationToken, () =>
                {
                    _popUpService.HidePopUp<CharacterChooseMenu>();
                    _popUpService.ShowPopUp<HUDMenu>();
                });
            }));
        }

        private void SpawnByCharacter(CharacterClass characterClass, PlayerRef playerRef,
                                            NetworkRunner networkRunner)
        {
            Debug.Log($"Player {playerRef} chose {characterClass}");
            SpawnPlayer(playerRef, characterClass, networkRunner);
        }

        private async void SpawnPlayer(PlayerRef playerRef, CharacterClass characterClass, NetworkRunner networkRunner)
        {   
            RPC_AssignTeam(playerRef);

            PlayerBase playerBase = networkRunner.Spawn(_playerBasePrefab, null, null, playerRef);
            playerBase.Object.AssignInputAuthority(playerRef);
            playerBase.CharacterClass = characterClass;
            
            RPC_AddPlayer(playerRef, _authService.MyProfile.Nickname, playerBase);
            
            _healthObjectsService.RegisterPlayer(playerRef);

            RPC_OnPlayerBaseSpawnedInvoke(playerBase);
            
            await UniTask.Delay(500);
            
            SpawnCharacter(playerRef, playerBase, characterClass);

            //LoadWeapon(player);
        }

        public void DespawnPlayer(PlayerRef playerRef, bool isLeftFromSession)
        {
            if (isLeftFromSession)
            {
                NetworkId id = playerRef.ToNetworkId();

                _sessionStateService.RemovePlayer(id);
                _teamsService.RPC_RemoveFromTeam(playerRef.ToNetworkId());
                _healthObjectsService.UnRegisterObject(id);
                PlayersBaseDictionary.Remove(playerRef);
                
                BaseDespawned.OnNext(playerRef);
            }
            else
            {
                PlayerCharacter playerCharacter = PlayersBaseDictionary[playerRef].Character;

                //_healthObjectsService.UnRegisterObject(playerCharacter.Object); // TODO
                
                Runner.Despawn(playerCharacter.Object);

                PlayersBaseDictionary[playerRef].Character = null;
                CharacterDespawned.OnNext(playerRef);
            }

            PlayerManager.PlayersOnServer.Remove(playerRef);
            PlayerManager.LoadingPlayers.Remove(playerRef);
        }

        private PlayerCharacter SpawnCharacter(PlayerRef playerRef ,PlayerBase playerBase, CharacterClass characterClass)
        {
            CharacterData characterData = _gameSettings.CharactersDataContainer.GetCharacterDataByClass(characterClass);

            var hasTeam = _sessionStateService.TryGetPlayerTeam(playerRef, out var teamSide);

            if (!hasTeam)
            {
                AtomicLogger.Err(hasTeam.ErrorMessage);
                return null;
            }
            
            Vector3 spawnPos = Extensions.AtomicCart.GetSpawnPosByTeam(teamSide);

            PlayerCharacter playerCharacter = Runner.Spawn(characterData.PlayerCharacterPrefab, spawnPos,
                quaternion.identity, playerRef, onBeforeSpawned: (runner, o) =>
                {
                    PlayerCharacter character = o.GetComponent<PlayerCharacter>();
                    SessionPlayer sessionPlayer = _sessionStateService.GetSessionPlayer(playerRef.ToNetworkId()); 
                    character.WeaponController.RPC_SetOwner(sessionPlayer);
                    character.transform.parent = playerBase.transform;

                    DiInjecter.Instance.InjectGameObject(o.gameObject);
                });

            NetworkObject playerNetObj = playerCharacter.Object;
            
            RPC_AssignPlayerCharacter(playerRef, playerCharacter, characterClass);
            PlayersBaseDictionary[playerRef].Character = playerCharacter;
            PlayersBaseDictionary[playerRef].CharacterClass = characterClass;
            PlayersBaseDictionary[playerRef].PlayerController.IsCastingMode = false;
            
            playerBase.AbilityCastController.ResetAbility();
            SetAbilityType(playerBase, characterClass);

            playerNetObj.RequestStateAuthority();
            playerNetObj.AssignInputAuthority(playerRef);
            Runner.SetPlayerObject(playerRef, playerNetObj);

            playerBase.PlayerController.Init(characterData.CharacterStats.MoveSpeed,
                _gameSettings.ShootThreshold, characterData.CharacterStats.SpeedLowerSpeed);

            playerBase.PlayerController.SetAllowToMove(true);
            playerBase.PlayerController.SetAllowToShoot(true);

            playerCharacter.RPC_Init(characterClass);

            PlayerSpawnEventContext spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.CharacterClass = characterClass;
            spawnEventContext.PlayerRef = playerRef;
            spawnEventContext.Transform = playerCharacter.transform;

            SetCharacterTeamBannerColor(playerRef);
            
            CharacterSpawned.OnNext(spawnEventContext);

            return playerCharacter;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AssignPlayerCharacter(PlayerRef playerRef, PlayerCharacter playerCharacter, CharacterClass characterClass)
        {
            PlayersBaseDictionary[playerRef].Character = playerCharacter;
            PlayersBaseDictionary[playerRef].CharacterClass = characterClass;
        }

        public void ChangePlayerCharacter(PlayerRef playerRef, CharacterClass newCharacterClass)
        {   
            DespawnPlayer(playerRef, false);
            SpawnCharacter(playerRef, PlayersBaseDictionary[playerRef], newCharacterClass);
            SetCharacterTeamBannerColor(playerRef);
        }
        
        private static void SetAbilityType(PlayerBase playerBase, CharacterClass characterClass)
        {
            AbilityType abilityType;

            switch (characterClass)
            {
                case CharacterClass.Soldier:
                    abilityType = AbilityType.MiniAirStrike;
                    break;
                case CharacterClass.Engineer:
                    abilityType = AbilityType.Turret;
                    break;
                case CharacterClass.Marine:
                    abilityType = AbilityType.TearGas;
                    break;
                case CharacterClass.Bomber:
                    abilityType = AbilityType.Landmine;
                    break;
                default:
                    abilityType = AbilityType.MiniAirStrike;
                    break;
            }

            playerBase.AbilityCastController.RPC_SetAbilityType(abilityType);
        }

        [Rpc]
        private void RPC_AddPlayer(PlayerRef playerRef, string nickname, PlayerBase playerBase)
        {
            PlayersBaseDictionary.Add(playerRef, playerBase);
            
            _sessionStateService.AddPlayer(playerRef.ToNetworkId(), $"{nickname}", false);
        }

        private void SetCharacterTeamBannerColor(PlayerRef playerRef)
        {
            var hasTeam = _sessionStateService.TryGetPlayerTeam(playerRef, out var teamSide);

            if (!hasTeam)
            {
                AtomicLogger.Err(hasTeam.ErrorMessage);
                return;
            }
            
            GetPlayer(playerRef).PlayerView.RPC_SetTeamColor(AtomicConstants.Teams.GetTeamColor(teamSide));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AssignTeam(PlayerRef playerRef)
        {
            int redTeamMembersCount = _teamsService.GetTeamMembersCount(TeamSide.Red);
            int blueTeamMembersCount = _teamsService.GetTeamMembersCount(TeamSide.Blue);

            var newTeamForPlayer = redTeamMembersCount > blueTeamMembersCount ? TeamSide.Blue : TeamSide.Red;

            Debug.Log($"Assigning team {newTeamForPlayer} for player {playerRef}");

            _teamsService.RPC_AssignForTeam(new TeamMember(playerRef), newTeamForPlayer);
        }

        public void RespawnPlayerCharacter(PlayerRef playerRef)
        {
            Debug.Log($"Respawn player {playerRef}");
            _healthObjectsService.RestorePlayerHealth(playerRef);

            var hasTeam = _sessionStateService.TryGetPlayerTeam(playerRef, out var playerTeamSide);

            if (!hasTeam)
            {
                AtomicLogger.Err(hasTeam.ErrorMessage);
                return;
            }

            var spawnPoints = LevelService.Instance.CurrentLevel.GetSpawnPointsByTeam(playerTeamSide);

            SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            PlayerCharacter playerCharacter = GetPlayer(playerRef);
            playerCharacter.RPC_SetPos(spawnPoint.transform.position);

            PlayerBase playerBase = GetPlayerBase(playerRef);

            Extensions.Delay(0.5f, destroyCancellationToken, () =>
            {
                playerCharacter.RPC_ResetAfterDeath();

                playerBase.PlayerController.SetAllowToMove(true);
                playerBase.PlayerController.SetAllowToShoot(true);
            });
            
            SetCharacterTeamBannerColor(playerRef);
        }

        public PlayerCharacter GetPlayer(PlayerRef playerRef)
        {
            return GetPlayerBase(playerRef).Character;
        }

        public PlayerBase GetPlayerBase(PlayerRef playerRef)
        {
            return PlayersBaseDictionary[playerRef];
        }

        public PlayerBase GetPlayerBase(NetworkId id)
        {
            return PlayersBaseDictionary.First(x => x.Value.Object.StateAuthority.ToNetworkId() == id).Value;
        }

        [Rpc]
        private void RPC_OnPlayerBaseSpawnedInvoke(PlayerBase playerBase)
        {
            PlayerRef playerRef = playerBase.Object.InputAuthority;

            var spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.PlayerRef = playerRef;
            spawnEventContext.Transform = playerBase.transform;
            spawnEventContext.CharacterClass = playerBase.CharacterClass;

            // Debug.Log($"[RPC] Player spawned");
            BaseSpawned.OnNext(spawnEventContext);
        }

        private void LoadWeapon(PlayerCharacter playerCharacter)
        {
            var weaponSetupContext = new WeaponSetupContext(WeaponType.Rifle);
            playerCharacter.WeaponController.Init(weaponSetupContext);
        }
    }
}