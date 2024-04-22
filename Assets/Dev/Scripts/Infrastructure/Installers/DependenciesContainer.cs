using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class DependenciesContainer 
    {
        private DiContainer _diContainer;

        public static DependenciesContainer Instance { get; private set; }

        public DependenciesContainer(DiContainer diContainer)
        {
            _diContainer = diContainer;

            Instance = this;
        }

        public void Inject(GameObject gameObject)
        {
            _diContainer.InjectGameObject(gameObject);
        }
        
        public TType GetDependency<TType>()
        {
            return _diContainer.Resolve<TType>();
        }

    }
}