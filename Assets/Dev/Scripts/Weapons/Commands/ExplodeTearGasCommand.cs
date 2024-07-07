using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public class ExplodeTearGasCommand : AbilityCastCommand
    {
        private TearGasService _tearGasService;

        public ExplodeTearGasCommand(NetworkRunner runner, AbilityType abilityType, SessionPlayer owner, TearGasService tearGasService) : base(runner, abilityType, owner)
        {
            _tearGasService = tearGasService;
        }

        public override void Process(Vector3 pos)
        {
            base.Process(pos);
            
            _tearGasService.ExplodeTearGas(_runner, pos, _owner);
            _tearGasService.DurationEnded.Subscribe((unit => OnTearGasEnded()));
        }

        private void OnTearGasEnded()
        {
           Reset();
        }
    }
}