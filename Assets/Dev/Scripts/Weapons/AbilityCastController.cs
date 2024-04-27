using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Dev.Weapons
{
    public class AbilityCastController : NetworkContext
    {
        public bool AllowToCast => _currentCastCommand == null ? true : _currentCastCommand.AllowToCast; 

        [FormerlySerializedAs("_turret")] [SerializeField] private Turret _turretPrefab;
        [SerializeField] private Landmine _landminePrefab;
        
        private AbilityCastCommand[] _castCommands;

        private AbilityCastCommand _currentCastCommand;
        
        private TeamsService _teamsService;
        private PlayerCharacter _playerCharacter;

        public Subject<AbilityType> AbilityRecharged { get; } = new();
        public Subject<AbilityType> AbilityChanged { get; } = new();
            
        [Inject]
        private void Construct(TeamsService teamsService, PlayerCharacter playerCharacter)
        {
            _playerCharacter = playerCharacter;
            _teamsService = teamsService;
        }

        public async override void Spawned()
        {
            if(HasStateAuthority == false) return;

            await UniTask.Delay(100);
            
            _castCommands = new AbilityCastCommand[2];

            _castCommands[0] = new PlaceTurretCastCommand(Runner, AbilityType.Turret, _turretPrefab);
            _castCommands[1] = new CastLandmineCommand(Runner, AbilityType.Landmine, _landminePrefab, _playerCharacter.TeamSide);
            
            foreach (AbilityCastCommand castCommand in _castCommands)
            {
                castCommand.AbilityRecharged.TakeUntilDestroy(this).Subscribe((OnAbilityRecharged));
            }
            
            //_placeTurretCastCommand = new PlaceTurretCastCommand(Runner, _turret);
        }

        public void CastAbility(AbilityType abilityType, Vector3 pos)
        {
            ResetAbility(abilityType);

            AbilityCastCommand command = GetCommand(abilityType);
            command.Process(pos);

            _currentCastCommand = command;
        }

        public void ResetAbility(AbilityType abilityType)
        {
            AbilityCastCommand command = GetCommand(abilityType);
            command.Reset();
        }

        private void OnAbilityRecharged(AbilityType abilityType)
        {
            Debug.Log($"Ability {abilityType} recharged!");
            AbilityRecharged.OnNext(abilityType);
        }

        private AbilityCastCommand GetCommand(AbilityType abilityType)
        {
            return _castCommands.First(x => x.AbilityType == abilityType);
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