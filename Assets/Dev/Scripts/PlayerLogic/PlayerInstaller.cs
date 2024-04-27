using Dev.Infrastructure;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerCharacter _playerCharacter;

        private void Reset()
        {
            _playerCharacter = GetComponent<PlayerCharacter>();
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
            Container.Bind<PlayerCharacter>().FromInstance(_playerCharacter).AsSingle();
        }
    }
}