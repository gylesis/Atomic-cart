using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public class CallAirStrikeCommand : AbilityCastCommand
    {
        private AirStrikeController _airStrikeController;

        public CallAirStrikeCommand(NetworkRunner runner, AbilityType abilityType, SessionPlayer owner, AirStrikeController airStrikeController) : base(runner, abilityType, owner)
        {
            _airStrikeController = airStrikeController;
        }
        
        public override void Process(Vector3 pos)
        {
            base.Process(pos);

            _airStrikeController.AirstrikeCompleted.Take(1).Subscribe((unit => OnAirStrikeCompleted()));
            
            _airStrikeController.CallMiniAirStrike(_runner, pos, _owner).Forget();
        }

        private void OnAirStrikeCompleted()
        {
            Reset();    
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}