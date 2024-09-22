using Dev.CartLogic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Dev.Infrastructure
{
    public class LevelInstaller : MonoInstaller
    {
        [FormerlySerializedAs("_cartPathService")] [SerializeField] private CartService cartService;
        
        public override void InstallBindings()
        {
            Container.Bind<CartService>().FromInstance(cartService).AsSingle();
        }
    }
}