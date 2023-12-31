﻿using Fusion;
using Zenject;

namespace Dev.Infrastructure
{
    public class DependenciesContainer
    {
        private DiContainer _diContainer;

        public static DependenciesContainer Instance { get; private set; }

        public DependenciesContainer(DiContainer diContainer)
        {
            if (Instance != null)
            {
                return;
            }

            _diContainer = diContainer;

            Instance = this;
        }

        public TType GetDependency<TType>()
        {
            return _diContainer.Resolve<TType>();
        }
    }
}