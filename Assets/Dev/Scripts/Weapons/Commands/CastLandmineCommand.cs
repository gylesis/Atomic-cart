using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public class CastLandmineCommand : AbilityCastCommand
    {
        private Landmine _landminePrefab;
        private TeamSide _ownerTeamSide;
        
        private Landmine _spawnedLandmine;

        public CastLandmineCommand(NetworkRunner runner, AbilityType abilityType, Landmine landminePrefab, TeamSide ownerTeamSide) : base(runner, abilityType)
        {
            _ownerTeamSide = ownerTeamSide;
            _landminePrefab = landminePrefab;
        }
        
        public override void Process(Vector3 pos)
        {
            base.Process(pos);
            
            _spawnedLandmine = Spawn(_landminePrefab, pos, _runner.LocalPlayer, (runner, o) =>
            {
                Landmine landmine = o.GetComponent<Landmine>();

                landmine.ToDestroy.TakeUntilDestroy(landmine).Subscribe((unit => OnLandmineDestroyed()));
                
                landmine.Init(_ownerTeamSide);
                landmine.Init(Vector2.zero, 0, 50, _runner.LocalPlayer);
            });
            
            AllowToCast = false;
        }

        private void OnLandmineDestroyed()
        {
            Reset();
        }

        public override void Reset()
        {
            if(_spawnedLandmine == null) return;

            base.Reset();
            
            _runner.Despawn(_spawnedLandmine.Object);
            
            _spawnedLandmine = null;
        }
    }
}