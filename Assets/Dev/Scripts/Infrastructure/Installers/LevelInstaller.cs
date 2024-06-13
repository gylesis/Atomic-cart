using Dev.CartLogic;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class LevelInstaller : MonoInstaller
    {
        [SerializeField] private CartPathService _cartPathService;
        
        public override void InstallBindings()
        {
            Container.Bind<CartPathService>().FromInstance(_cartPathService).AsSingle();
        }
    }
}