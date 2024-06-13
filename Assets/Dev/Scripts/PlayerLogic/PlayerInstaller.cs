using Dev.Infrastructure;
using Dev.Weapons;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerBase _playerBase;
        [SerializeField] private AbilityCastController _abilityCastController;
        [SerializeField] private PlayerController _playerController;
        
        private void Reset()
        {
            _playerBase = GetComponent<PlayerBase>();
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
            
            Container.Bind<PlayerBase>().FromInstance(_playerBase).AsSingle();
            Container.Bind<AbilityCastController>().FromInstance(_abilityCastController).AsSingle();
            Container.Bind<PlayerController>().FromInstance(_playerController).AsSingle();
        }
        
    }
}