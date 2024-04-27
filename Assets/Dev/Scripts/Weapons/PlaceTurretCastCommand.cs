using Dev.Infrastructure;
using Fusion;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Dev.Weapons
{
    public class PlaceTurretCastCommand : AbilityCastCommand
    {
        private Turret _turretPrefab;
        
        private Turret _spawnedTurret;
        
        public bool AllowToCast { get; private set; }

        public PlaceTurretCastCommand(NetworkRunner runner, AbilityType abilityType, Turret turretPrefab) : base(runner, abilityType)
        {
            _turretPrefab = turretPrefab;
        }   

        public override void Process(Vector3 pos)
        {
            PlayerRef localPlayer = _runner.LocalPlayer;
            
            _spawnedTurret = _runner.Spawn(_turretPrefab, pos, inputAuthority: localPlayer, onBeforeSpawned: (runner, o) =>
            {
                Turret turret = o.GetComponent<Turret>();

                turret.OnDestroyAsObservable().Subscribe((unit => OnTurretDestroyed(turret)));
                
                turret.Init(localPlayer);
            });

            AllowToCast = false;
        }

        private void OnTurretDestroyed(Turret turret)
        {
            Reset();
        }

        public override void Reset()
        {
            if(_spawnedTurret == null) return;
            
            _runner.Despawn(_spawnedTurret.Object);
            
            _spawnedTurret = null;
            AllowToCast = true;
            
            AbilityRecharged.OnNext(AbilityType);
        }
    }
    
    
    
    
}