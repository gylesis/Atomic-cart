using Dev.Infrastructure;
using Fusion;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Dev.Weapons.Commands
{
    public class PlaceTurretCastCommand : AbilityCastCommand
    {
        private Turret _turretPrefab;
        
        private Turret _spawnedTurret;
        
        public bool AllowToCast { get; private set; }

        public PlaceTurretCastCommand(NetworkRunner runner, AbilityType abilityType, SessionPlayer owner, Turret turretPrefab) : base(runner, abilityType, owner)
        {
            _turretPrefab = turretPrefab;
        }   

        public override void Process(Vector3 pos)
        {
            base.Process(pos);
            
            PlayerRef localPlayer = _runner.LocalPlayer;
            
            _spawnedTurret = _runner.Spawn(_turretPrefab, pos, inputAuthority: localPlayer, onBeforeSpawned: (runner, o) =>
            {
                Turret turret = o.GetComponent<Turret>();

                turret.OnDestroyAsObservable().Subscribe((unit => OnTurretDestroyed(turret)));
                
                turret.Init(_owner);
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
            
            base.Reset();
            
            _runner.Despawn(_spawnedTurret.Object);
            
            _spawnedTurret = null;
        }
    }
}