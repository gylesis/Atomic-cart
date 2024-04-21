using Dev.Infrastructure;
using Fusion;
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
        
    }
    

    public enum AbilityType
    {
        Landmine,
        MiniAirStrike,
        Turret,
        TearGas
    }

    public class AbilityCaster
    {
            
        
        
    }

    public class AbilityCastCommand
    {


        public void Proccess()
        {
            
        }
    }


    public class PlaceTurretCastCommand
    {
        private NetworkRunner _runner;
        private Turret _turretPrefab;

        public PlaceTurretCastCommand(NetworkRunner runner, Turret turretPrefab)
        {
            _turretPrefab = turretPrefab;
            _runner = runner;
        }

        public void Proccess(Vector3 pos)
        {
            _runner.Spawn(_turretPrefab, pos, inputAuthority: _runner.LocalPlayer, onBeforeSpawned: (runner, o) =>
            {
                Turret turret = o.GetComponent<Turret>();

                turret.Init(_runner.LocalPlayer);
            });
        }
        
    }
}