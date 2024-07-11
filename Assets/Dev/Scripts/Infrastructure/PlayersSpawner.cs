﻿using System;
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

        [Inject]
        private void Init(TeamsService teamsService, PopUpService popUpService,
                          GameStaticDataContainer gameStaticDataContainer, SessionStateService sessionStateService,
                          HealthObjectsService healthObjectsService)
        {
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
            _popUpService.TryGetPopUp<CharacterChooseMenu>(out var characterChooseMenu);

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

            RPC_AddPlayer(playerRef, playerBase);

            await UniTask.Delay(500);
            
            PlayerCharacter playerCharacter = SpawnCharacter(playerRef, playerBase, characterClass);

            UpdatePlayerCharacter(playerCharacter, playerRef);

            RPC_OnPlayerSpawnedInvoke(playerBase);

            //LoadWeapon(player);
        }

        public void DespawnPlayer(PlayerRef playerRef, bool isLeftFromSession)
        {
            if (isLeftFromSession)
            {
                _sessionStateService.RPC_RemovePlayer(GetPlayerBase(playerRef).Object.Id);
                _teamsService.RPC_RemoveFromTeam(playerRef);
                _healthObjectsService.RPC_UnregisterObject(PlayersBase[playerRef].Character.Object);
                PlayersBase.Remove(playerRef);
                
                PlayerBaseDeSpawned.OnNext(playerRef);
            }
            else
            {
                PlayerCharacter playerCharacter = PlayersBase[playerRef].Character;

                _healthObjectsService.RPC_UnregisterObject(playerCharacter.Object);
                
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

            TeamSide teamSide = _teamsService.GetUnitTeamSide(playerRef);
            Vector3 spawnPos = Extensions.AtomicCart.GetSpawnPosByTeam(teamSide);

            PlayerCharacter playerCharacter = Runner.Spawn(characterData.PlayerCharacterPrefab, spawnPos,
                quaternion.identity, playerRef, onBeforeSpawned: (runner, o) =>
                {
                    PlayerCharacter character = o.GetComponent<PlayerCharacter>();
                    character.WeaponController.RPC_SetOwner(_sessionStateService.GetSessionPlayer(playerRef));
                    character.transform.parent = playerBase.transform;

                    DependenciesContainer.Instance.Inject(o.gameObject);
                });

            NetworkObject playerNetObj = playerCharacter.Object;

            PlayersBase[playerRef].Character = playerCharacter;
            
            playerBase.AbilityCastController.ResetAbility();
            SetAbilityType(playerBase, characterClass);

            playerNetObj.RequestStateAuthority();
            playerNetObj.AssignInputAuthority(playerRef);
            Runner.SetPlayerObject(playerRef, playerNetObj);

            playerBase.PlayerController.Init(characterData.CharacterStats.MoveSpeed,
                characterData.CharacterStats.ShootThreshold, characterData.CharacterStats.SpeedLowerSpeed);

            playerBase.PlayerController.SetAllowToMove(true);
            playerBase.PlayerController.SetAllowToShoot(true);

            playerCharacter.RPC_Init(characterClass, teamSide);

            UpdatePlayerCharacter(playerCharacter, playerRef);

            PlayerSpawnEventContext spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.CharacterClass = characterClass;
            spawnEventContext.PlayerRef = playerRef;
            spawnEventContext.Transform = playerCharacter.transform;

            _healthObjectsService.RegisterPlayer(playerRef);
            
            PlayerCharacterSpawned.OnNext(spawnEventContext);

            return playerCharacter;
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

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddPlayer(PlayerRef playerRef, PlayerBase playerBase)
        {
            PlayersBase.Add(playerRef, playerBase);
            
            _sessionStateService.RPC_AddPlayer(playerBase.Object.Id, "Player", false, _teamsService.GetUnitTeamSide(playerRef));
        }

        private void SetCharacterTeamBannerColor(PlayerRef playerRef)
        {
            TeamSide teamSide = _teamsService.GetUnitTeamSide(playerRef);

            GetPlayer(playerRef).PlayerView.RPC_SetTeamColor(AtomicConstants.Teams.GetTeamColor(teamSide));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AssignTeam(PlayerRef playerRef)
        {
            bool doPlayerHasTeam = _teamsService.DoPlayerHasTeam(playerRef);

            if (doPlayerHasTeam) return;

            TeamSide newTeamForPlayer;

            int redTeamMembersCount = _teamsService.GetTeamMembersCount(TeamSide.Red);
            int blueTeamMembersCount = _teamsService.GetTeamMembersCount(TeamSide.Blue);

            if (redTeamMembersCount > blueTeamMembersCount)
            {
                newTeamForPlayer = TeamSide.Blue;
            }
            else
            {
                newTeamForPlayer = TeamSide.Red;
            }

            Debug.Log($"Assigning team {newTeamForPlayer} for player {playerRef}");

            _teamsService.RPC_AssignForTeam(playerRef, newTeamForPlayer);
        }

        private void SetCamera(PlayerRef playerRef, Vector3 pos, NetworkRunner networkRunner)
        {
            CameraController cameraController = networkRunner.Spawn(_cameraControllerPrefab,
                pos,
                Quaternion.identity,
                playerRef, onBeforeSpawned: (runner, o) => { DependenciesContainer.Instance.Inject(o.gameObject); });

            cameraController.SetFollowState(true);
            cameraController.Object.RequestStateAuthority();
            _playerServices[playerRef].Add(cameraController.Object);
        }

        public void RespawnPlayerCharacter(PlayerRef playerRef)
        {
            Debug.Log($"Respawn player {playerRef}");
            _healthObjectsService.RestorePlayerHealth(playerRef);

            TeamSide playerTeamSide = _teamsService.GetUnitTeamSide(playerRef);

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

        private void LoadWeapon(PlayerCharacter playerCharacter)
        {
            var weaponSetupContext = new WeaponSetupContext(WeaponType.Rifle);
            playerCharacter.WeaponController.Init(weaponSetupContext);
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
            return PlayersBase.First(x => x.Value.Object.Id == id).Value;
        }

        public CameraController GetPlayerCameraController(PlayerRef playerRef)
        {
            List<NetworkObject> playerService = _playerServices[playerRef];

            CameraController cameraController = playerService.First(x => x.GetComponent<CameraController>() != null)
                .GetComponent<CameraController>();

            return cameraController;
        }

        public Vector3 GetPlayerPos(PlayerRef playerRef) => GetPlayer(playerRef).transform.position;


        [Rpc]
        private void RPC_OnPlayerSpawnedInvoke(PlayerBase playerBase)
        {
            PlayerRef playerRef = playerBase.Object.InputAuthority;

            var spawnEventContext = new PlayerSpawnEventContext();
            spawnEventContext.PlayerRef = playerRef;
            spawnEventContext.Transform = playerBase.transform;
            spawnEventContext.CharacterClass = playerBase.CharacterClass;

            // Debug.Log($"[RPC] Player spawned");
            PlayerBaseSpawned.OnNext(spawnEventContext);
        }
    }
}