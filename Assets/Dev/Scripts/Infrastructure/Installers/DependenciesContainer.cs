using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class DependenciesContainer 
    {
        private DiContainer _diContainer;

        public static DependenciesContainer Instance { get; private set; }

        private HashSet<int> _objects = new HashSet<int>(16);
        
        public DependenciesContainer(DiContainer diContainer)
        {
            _diContainer = diContainer;

            Instance = this;
        }

        public void Inject(GameObject gameObject)
        {
            if(_objects.Contains(gameObject.GetInstanceID())) return;

            gameObject.OnDestroyAsObservable().Subscribe((unit => _objects.Remove(gameObject.GetInstanceID())));
            _objects.Add(gameObject.GetInstanceID());
            _diContainer.InjectGameObject(gameObject);
        }
        
        /*public TType GetDependency<TType>()
        {
            return _diContainer.Resolve<TType>();
        }*/

    }
}