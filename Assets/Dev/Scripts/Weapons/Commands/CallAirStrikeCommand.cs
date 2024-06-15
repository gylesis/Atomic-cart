using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public class CallAirStrikeCommand : AbilityCastCommand
    {
        private AirStrikeController _airStrikeController;

        public CallAirStrikeCommand(NetworkRunner runner, AbilityType abilityType, TeamSide teamSide, AirStrikeController airStrikeController) : base(runner, abilityType, teamSide)
        {
            _airStrikeController = airStrikeController;
        }
        
        public override void Process(Vector3 pos)
        {
            base.Process(pos);

            _airStrikeController.AirstrikeCompleted.Take(1).Subscribe((unit => OnAirStrikeCompleted()));
            
            _airStrikeController.CallMiniAirStrike(pos, _teamSide);
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