﻿using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Weapons
{
    public class AbilityCastController : NetworkContext
    {
        [SerializeField] private Turret _turretPrefab;
        [SerializeField] private Landmine _landminePrefab;
        
        private List<AbilityCastCommand> _castCommands = new List<AbilityCastCommand>(4);
        private AbilityCastCommand _currentCastCommand;
        
        private PlayerBase _playerBase;
        private AirStrikeController _airStrikeController;

        public bool AllowToCast => _currentCastCommand == null ? true : _currentCastCommand.AllowToCast;
        [Networked, HideInInspector] public AbilityType CurrentAbilityToCast { get; private set; }
        
        public Subject<AbilityType> AbilityRecharged { get; } = new();
        public Subject<AbilityType> AbilityChanged { get; } = new();
            
        [Inject]
        private void Construct(PlayerBase playerBase, AirStrikeController airStrikeController)
        {
            _airStrikeController = airStrikeController;
            _playerBase = playerBase;
        }

        [Rpc]
        public void RPC_SetAbilityType(AbilityType abilityType)
        {
            CurrentAbilityToCast = abilityType;
        }

        public async override void Spawned()
        {
            if(HasStateAuthority == false) return;

            await UniTask.Delay(100);

            Debug.Log("Init cast commands");
            _castCommands.Add(new PlaceTurretCastCommand(Runner, AbilityType.Turret, _turretPrefab));
            _castCommands.Add(new CastLandmineCommand(Runner, AbilityType.Landmine, _landminePrefab, _playerBase.TeamSide));
            _castCommands.Add(new CallAirStrikeCommand(Runner, AbilityType.MiniAirStrike, _airStrikeController, _playerBase.TeamSide));
            
            foreach (AbilityCastCommand castCommand in _castCommands)
            {
                castCommand.AbilityRecharged.TakeUntilDestroy(this).Subscribe((OnAbilityRecharged));
            }
        }

        public void CastAbility(Vector3 pos)
        {
            ResetAbility();

            AbilityCastCommand command = GetCommand(CurrentAbilityToCast);
            command.Process(pos);

            Debug.Log($"About to cast {CurrentAbilityToCast}");
            
            _currentCastCommand = command;
        }

        public void ResetAbility()
        {
            if(_castCommands.Count == 0) return;
            
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