using Dev.Infrastructure;
using Fusion;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Dev.Weapons
{
    public class AbilityCastController : NetworkContext
    {
        [Networked, HideInInspector] public bool AllowToCast { get; set; } = true;
       
        [SerializeField] private Turret _turret;
        
        private PlaceTurretCastCommand _placeTurretCastCommand;

        public override void Spawned()
        {
            if(HasStateAuthority == false) return;

            _placeTurretCastCommand = new PlaceTurretCastCommand(Runner, _turret);
        }

        public void CastAbility(AbilityType abilityType, Vector3 pos)
        {
            ResetAbility(abilityType);
            
            switch (abilityType)
            {
                case AbilityType.Landmine:
                    break;
                case AbilityType.MiniAirStrike:
                    break;
                case AbilityType.Turret:
                    _placeTurretCastCommand.Proccess(pos);
                    break;
                case AbilityType.TearGas:
                    break;
            }
        }

        public void ResetAbility(AbilityType abilityType)
        {
            switch (abilityType)
            {
                case AbilityType.Landmine:
                    break;
                case AbilityType.MiniAirStrike:
                    break;
                case AbilityType.Turret:
                    _placeTurretCastCommand.Reset();
                    break;
                case AbilityType.TearGas:
                    break;
            }
        }
        
    }

    public enum AbilityType
    {
        Landmine,
        MiniAirStrike,
        Turret,
        TearGas
    }

    public abstract class AbilityCastCommand
    {
        public abstract void Proccess(Vector3 pos);
        public abstract void Reset();
    }


    public class PlaceTurretCastCommand : AbilityCastCommand
    {
        private NetworkRunner _runner;
        private Turret _turretPrefab;
        private Turret _spawnedTurret;
        
        public bool AllowToCast { get; private set; }

        public PlaceTurretCastCommand(NetworkRunner runner, Turret turretPrefab)
        {
            _turretPrefab = turretPrefab;
            _runner = runner;
        }

        public override void Proccess(Vector3 pos)
        {
            PlayerRef localPlayer = _runner.LocalPlayer;
            
            _spawnedTurret = _runner.Spawn(_turretPrefab, pos, inputAuthority: localPlayer, onBeforeSpawned: (runner, o) =>
            {
                Turret turret = o.GetComponent<Turret>();

                DependenciesContainer.Instance.Inject(turret.gameObject);
                turret.OnDestroyAsObservable().Subscribe((unit => OnTurretDestroyed(turret)));
                
                turret.Init(localPlayer);
            });

            AllowToCast = false;
        }

        private void OnTurretDestroyed(Turret turret)
        {
           // _spawnedTurret = null;
        }

        public override void Reset()
        {
            if(_spawnedTurret == null) return;
            
            _runner.Despawn(_spawnedTurret.Object);
            
            _spawnedTurret = null;
            AllowToCast = true;
        }
    }
}