using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public class ExplodeTearGasCommand : AbilityCastCommand
    {
        private TearGasService _tearGasService;

        public ExplodeTearGasCommand(NetworkRunner runner, AbilityType abilityType, TeamSide teamSide, TearGasService tearGasService) : base(runner, abilityType, teamSide)
        {
            _tearGasService = tearGasService;
        }

        public override void Process(Vector3 pos)
        {
            base.Process(pos);
            
            _tearGasService.ExplodeTearGas(pos, _teamSide);
            _tearGasService.DurationEnded.Subscribe((unit => OnTearGasEnded()));
        }

        private void OnTearGasEnded()
        {
           Reset();
        }
    }
}