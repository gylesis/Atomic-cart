﻿using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons.Guns;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Weapons.Commands
{
    public class AbilityCastController : NetworkContext
    {
        [SerializeField] private Turret _turretPrefab;
        [SerializeField] private Landmine _landminePrefab;
        
        private List<AbilityCastCommand> _castCommands = new List<AbilityCastCommand>(4);
        private AbilityCastCommand _currentCastCommand;
        
        private PlayerBase _playerBase;
        private AirStrikeController _airStrikeController;
        private GameStateService _gameStateService;
        private TearGasService _tearGasService;
        private SessionStateService _sessionStateService;

        public bool AllowToCast => _currentCastCommand == null ? true : _currentCastCommand.AllowToCast;
        [Networked, HideInInspector] public AbilityType CurrentAbilityToCast { get; private set; }
        
        public Subject<AbilityType> AbilityRecharged { get; } = new();
        public Subject<AbilityType> AbilityChanged { get; } = new();
            
        [Inject]
        private void Construct(PlayerBase playerBase, AirStrikeController airStrikeController, GameStateService gameStateService, TearGasService tearGasService, SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
            _tearGasService = tearGasService;
            _gameStateService = gameStateService;
            _airStrikeController = airStrikeController;
            _playerBase = playerBase;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();

            _gameStateService.GameRestarted.Subscribe((unit => OnGameRestarted()));
        }

        private void OnGameRestarted()
        {
            ResetAbility();
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_SetAbilityType(AbilityType abilityType)
        {
            CurrentAbilityToCast = abilityType;
        }

        public override async void Spawned()
        {
            base.Spawned();
            
            if(HasStateAuthority == false) return;

            await UniTask.WaitUntil((() => _playerBase.Character != null)).AttachExternalCancellation(gameObject.GetCancellationTokenOnDestroy());
            
            SessionPlayer owner = _sessionStateService.GetSessionPlayer(_playerBase.Object.StateAuthority.ToNetworkId());

            _castCommands.Add(new PlaceTurretCastCommand(Runner, AbilityType.Turret,  owner, _turretPrefab));
            _castCommands.Add(new CastLandmineCommand(Runner, AbilityType.Landmine, owner, _landminePrefab));
            _castCommands.Add(new CallAirStrikeCommand(Runner, AbilityType.MiniAirStrike, owner, _airStrikeController));
            _castCommands.Add(new ExplodeTearGasCommand(Runner, AbilityType.TearGas, owner, _tearGasService, _playerBase));
            
            foreach (AbilityCastCommand castCommand in _castCommands)
            {
                castCommand.AbilityRecharged.TakeUntilDestroy(this).Subscribe((OnAbilityRecharged));
            }
        }

        public void TryCastAbility(Vector3 pos)
        {
            if (AllowToCast == false)
            {
                AtomicLogger.Log("Not Allowed to cast ability");
                return;
            }
            
            ResetAbility();

            AbilityCastCommand command = GetCommand(CurrentAbilityToCast);
            
            
            
            command.Process(pos);

            Debug.Log($"About to cast {CurrentAbilityToCast}");
            
            _currentCastCommand = command;
        }
        
        public void ResetAbility()
        {
            if(_castCommands.Count == 0) return;

           // Debug.Log($"Reset ability {CurrentAbilityToCast}");
            _currentCastCommand = null;
            AbilityCastCommand command = GetCommand(CurrentAbilityToCast);
            command.Reset();
        }

        private void OnAbilityRecharged(AbilityType abilityType)
        {
            _currentCastCommand = null;
            Debug.Log($"Ability {abilityType} recharged!");
            AbilityRecharged.OnNext(abilityType);
        }

        private AbilityCastCommand GetCommand(AbilityType abilityType)
        {
            return _castCommands.First(x => x.AbilityType == abilityType);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);

            _castCommands = null;
        }
    }

    public enum AbilityType
    {
        Landmine,
        MiniAirStrike,
        Turret,
        TearGas
    }
}