using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private CharactersDataContainer _charactersDataContainer;
        
        public override void InstallBindings()
        {
            Container.Bind<CharactersDataContainer>().FromInstance(_charactersDataContainer).AsSingle();
        }
    }
}