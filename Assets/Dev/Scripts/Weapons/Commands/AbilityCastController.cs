using System.Collections.Generic;
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
        [SerializeField] private AbilityType _currentAbilityToCast;
        
        private List<AbilityCastCommand> _castCommands = new List<AbilityCastCommand>(4);

        private AbilityCastCommand _currentCastCommand;
        
        private PlayerCharacter _playerCharacter;
        private AirStrikeController _airStrikeController;

        public bool AllowToCast => _currentCastCommand == null ? true : _currentCastCommand.AllowToCast;
        public AbilityType CurrentAbilityToCast => _currentAbilityToCast;
        
        public Subject<AbilityType> AbilityRecharged { get; } = new();
        public Subject<AbilityType> AbilityChanged { get; } = new();
            
        [Inject]
        private void Construct(PlayerCharacter playerCharacter, AirStrikeController airStrikeController)
        {
            _airStrikeController = airStrikeController;
            _playerCharacter = playerCharacter;
        }

        public async override void Spawned()
        {
            if(HasStateAuthority == false) return;

            await UniTask.Delay(100);

            Debug.Log("Init cast commands");
            _castCommands.Add(new PlaceTurretCastCommand(Runner, AbilityType.Turret, _turretPrefab));
            _castCommands.Add(new CastLandmineCommand(Runner, AbilityType.Landmine, _landminePrefab, _playerCharacter.TeamSide));
            _castCommands.Add(new CallAirStrikeCommand(Runner, AbilityType.MiniAirStrike, _airStrikeController, _playerCharacter.TeamSide));
            
            foreach (AbilityCastCommand castCommand in _castCommands)
            {
                castCommand.AbilityRecharged.TakeUntilDestroy(this).Subscribe((OnAbilityRecharged));
            }
        }

        public void CastAbility(Vector3 pos)
        {
            ResetAbility();

            AbilityCastCommand command = GetCommand(_currentAbilityToCast);
            command.Process(pos);

            Debug.Log($"About to cast {_currentAbilityToCast}");
            
            _currentCastCommand = command;
        }

        public void ResetAbility()
        {
            _currentCastCommand = null;
            AbilityCastCommand command = GetCommand(_currentAbilityToCast);
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