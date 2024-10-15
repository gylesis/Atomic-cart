using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using Dev.Weapons;
using Dev.Weapons.StaticData;
using Fusion;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Zenject;

namespace Dev.Infrastructure
{
    public class PlayersSpawner : NetworkContext
    {
        [SerializeField] private CameraController _cameraControllerPrefab;
        [SerializeField] private PlayerBase _playerBasePrefab;

        public Subject<PlayerSpawnEventContext> PlayerBaseSpawned { get; } = new Subject<PlayerSpawnEventContext>();
        public Subject<PlayerSpawnEventContext> PlayerCharacterSpawned { get; } =
            new Subject<PlayerSpawnEventContext>();

        public Subject<PlayerRef> CharacterDeSpawned { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> PlayerBaseDeSpawned { get; } = new Subject<PlayerRef>();

        [Networked, Capacity(10)] private NetworkDictionary<PlayerRef, PlayerBase> PlayersBase { get; }

        public List<PlayerCharacter> PlayersCharacters => PlayersBase.Select(x => x.Value.Character).ToList();
        public List<PlayerBase> PlayersBases => PlayersBase.Select(x => x.Value).ToList();

        public int PlayersCount => PlayersBase.Count;

        private Dictionary<PlayerRef, List<NetworkObject>> _playerServices =
            new Dictionary<PlayerRef, List<NetworkObject>>();

        private TeamsService _teamsService;
        private PopUpService _popUpService;
        private CharactersDataContainer _charactersDataContainer;
        private SessionStateService _sessionStateService;
        private HealthObjectsService _healthObjectsService;
        private GameSettings _gameSettings;

        [Inject]
        private void Init(TeamsService teamsService, PopUpService popUpService,
                          GameStaticDataContainer gameStaticDataContainer, SessionStateService sessionStateService,
                          HealthObjectsService healthObjectsService, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _healthObjectsService = healthObjectsService;
            _sessionStateService = sessionStateService;
            _charactersDataContainer = gameStaticDataContainer.CharactersDataContainer;
            _popUpService = popUpService;
            _teamsService = teamsService;
        }

        public void ChooseCharacterClass(PlayerRef playerRef)
        {
            GetCharacterClassAndSpawn(playerRef);
        }

        private void GetCharacterClassAndSpawn(PlayerRef playerRef)
        {
            var characterChooseMenu = _popUpService.ShowPopUp<CharacterChooseMenu>();

            characterChooseMenu.StartChoosingCharacter((characterClass =>
            {
                SpawnPlayerByCharacter(characterClass, playerRef, Runner);

                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe((l =>
                {
                    _popUpService.HidePopUp<CharacterChooseMenu>();
                    _popUpService.ShowPopUp<HUDMenu>();
                }));
            }));
        }

        private void SpawnPlayerByCharacter(CharacterClass characterClass, PlayerRef playerRef,
                                            NetworkRunner networkRunner)
        {
            Debug.Log($"Player {playerRef} chose {characterClass}");

            SpawnPlayer(playerRef, characterClass, networkRunner);
        }

        private async void SpawnPlayer(PlayerRef playerRef, CharacterClass characterClass, NetworkRunner networkRunner)
        {   
            _playerServices.Add(playerRef, new List<NetworkObject>());
            SetCamera(playerRef, Vector3.zero, networkRunner);

            RPC_AssignTeam(playerRef);

            PlayerBase playerBase = networkRunner.Spawn(_playerBasePrefab, null, null, playerRef);
            playerBase.Object.AssignInputAuthority(playerRef);
            playerBase.CharacterClass = characterClass;
            
            RPC_AddPlayer(playerRef, playerBase);
            
            _healthObjectsService.RegisterPlayer(playerRef);

            RPC_OnPlayerBaseSpawnedInvoke(playerBase);
            
            await UniTask.Delay(500);
            
            PlayerCharacter playerCharacter = SpawnCharacter(playerRef, playerBase, characterClass);

            UpdatePlayerCharacter(playerCharacter, playerRef);

            //LoadWeapon(player);
        }

        public void DespawnPlayer(PlayerRef playerRef, bool isLeftFromSession)
        {
            if (isLeftFromSession)
            {
                NetworkId id = playerRef.ToNetworkId();

                _sessionStateService.RPC_RemovePlayer(id);
                _teamsService.RPC_RemoveFromTeam(playerRef.ToNetworkId());
                _healthObjectsService.RPC_UnregisterObject(id);
                PlayersBase.Remove(playerRef);
                
                PlayerBaseDeSpawned.OnNext(playerRef);
            }
            else
            {
                PlayerCharacter playerCharacter = PlayersBase[playerRef].Character;

                //_healthObjectsService.RPC_UnregisterObject(playerCharacter.Object); // TODO
                
                Runner.Despawn(playerCharacter.Object);

                PlayersBase[playerRef].Character = null;
                CharacterDeSpawned.OnNext(playerRef);
            }

            PlayerManager.PlayersOnServer.Remove(playerRef);
            PlayerManager.LoadingPlayers.Remove(playerRef);
        }

        private PlayerCharacter SpawnCharacter(PlayerRef playerRef ,PlayerBase playerBase, CharacterClass characterClass)
        {
            CharacterData characterData = _charactersDataContainer.GetCharacterDataByClass(characterClass);

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
            PlayersBase[playerRef].Character = playerCharacter;
            PlayersBase[playerRef].CharacterClass = characterClass;
            PlayersBase[playerRef].PlayerController.IsCastingMode = false;
            
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

            UpdatePlayerCharacter(playerCharacter, playerRef);

            PlayerSpawnEventContext spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.CharacterClass = characterClass;
            spawnEventContext.PlayerRef = playerRef;
            spawnEventContext.Transform = playerCharacter.transform;

            SetCharacterTeamBannerColor(playerRef);
            
            PlayerCharacterSpawned.OnNext(spawnEventContext);

            return playerCharacter;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AssignPlayerCharacter(PlayerRef playerRef, PlayerCharacter playerCharacter,
                                               CharacterClass characterClass)
        {
            PlayersBase[playerRef].Character = playerCharacter;
            PlayersBase[playerRef].CharacterClass = characterClass;
        }

        public void ChangePlayerCharacter(PlayerRef playerRef, CharacterClass newCharacterClass)
        {   
            DespawnPlayer(playerRef, false);
            PlayerCharacter playerCharacter = SpawnCharacter(playerRef, PlayersBase[playerRef], newCharacterClass);
            UpdatePlayerCharacter(playerCharacter, playerRef);
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

        private void UpdatePlayerCharacter(PlayerCharacter playerCharacter, PlayerRef playerRef)
        {
            CameraController cameraController = GetPlayerCameraController(playerRef);

            cameraController.SetupTarget(playerCharacter.transform);
            cameraController.FastSetOnTarget();
            cameraController.SetFollowState(true);
        }

        [Rpc]
        private void RPC_AddPlayer(PlayerRef playerRef, PlayerBase playerBase)
        {
            PlayersBase.Add(playerRef, playerBase);
            
            //_sessionStateService.AddPlayer(playerRef.ToNetworkId(), $"{PlayersLocalDataLinker.Instance.GetNickname(playerRef)}", false, _teamsService.GetUnitTeamSide(playerRef));
            _sessionStateService.AddPlayer(playerRef.ToNetworkId(), $"{AuthService.Nickname}", false);
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

        private void SetCamera(PlayerRef playerRef, Vector3 pos, NetworkRunner networkRunner)
        {
            CameraController cameraController = networkRunner.Spawn(_cameraControllerPrefab,
                pos,
                Quaternion.identity,
                playerRef, onBeforeSpawned: (runner, o) => { DiInjecter.Instance.InjectGameObject(o.gameObject); });

            cameraController.SetFollowState(true);
            cameraController.Object.RequestStateAuthority();
            _playerServices[playerRef].Add(cameraController.Object);
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

            Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe((l =>
            {
                playerCharacter.RPC_ResetAfterDeath();

                playerBase.PlayerController.SetAllowToMove(true);
                playerBase.PlayerController.SetAllowToShoot(true);
            }));

            SetCharacterTeamBannerColor(playerRef);
        }

        public PlayerCharacter GetPlayer(PlayerRef playerRef)
        {
            return GetPlayerBase(playerRef).Character;
        }

        public PlayerBase GetPlayerBase(PlayerRef playerRef)
        {
            return PlayersBase[playerRef];
        }

        public PlayerBase GetPlayerBase(NetworkId id)
        {
            return PlayersBase.First(x => x.Value.Object.StateAuthority.ToNetworkId() == id).Value;
        }

        public CameraController GetPlayerCameraController(PlayerRef playerRef)
        {
            List<NetworkObject> playerService = _playerServices[playerRef];

            CameraController cameraController = playerService.First(x => x.GetComponent<CameraController>() != null)
                .GetComponent<CameraController>();

            return cameraController;
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
            PlayerBaseSpawned.OnNext(spawnEventContext);
        }

        private void LoadWeapon(PlayerCharacter playerCharacter)
        {
            var weaponSetupContext = new WeaponSetupContext(WeaponType.Rifle);
            playerCharacter.WeaponController.Init(weaponSetupContext);
        }
    }
}