using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons.Commands
{
    public class ExplodeTearGasCommand : AbilityCastCommand
    {
        private TearGasService _tearGasService;
        private PlayerBase _playerBase;

        public ExplodeTearGasCommand(NetworkRunner runner, AbilityType abilityType, SessionPlayer owner, TearGasService tearGasService, PlayerBase playerBase) : base(runner, abilityType, owner)
        {
            _playerBase = playerBase;
            _tearGasService = tearGasService;
        }

        public override void Process(Vector3 pos)
        {
            base.Process(pos);
            
            _tearGasService.ExplodeTearGas(_runner, _playerBase.Character.transform.position, pos, _owner);
            _tearGasService.DurationEnded.Subscribe((unit => OnTearGasEnded()));
        }

        private void OnTearGasEnded()
        {
           Reset();
        }
    }
}