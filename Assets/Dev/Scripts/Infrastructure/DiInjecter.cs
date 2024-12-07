using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class DiInjecter
    {
        public DiContainer ProjectDiContainer { get; private set; }
        public DiContainer SceneDiContainer { get; private set; }

        private HashSet<int> _objects = new HashSet<int>(16);
        
        public static DiInjecter Instance { get; private set; }

        public DiInjecter(DiContainer diContainer)
        {
            ProjectDiContainer = diContainer;

            Instance = this;
        }

        public void LoadSceneDiContainer(DiContainer sceneDiContainer)
        {
            SceneDiContainer = sceneDiContainer;
        }

        public void InjectGameObject(GameObject gameObject)
        {
            if(_objects.Contains(gameObject.GetInstanceID())) return;

            if(SceneDiContainer == null) return; // it means its ProjectContext
            
            gameObject.OnDestroyAsObservable().Subscribe((unit => _objects.Remove(gameObject.GetInstanceID())));
            _objects.Add(gameObject.GetInstanceID());
            SceneDiContainer.InjectGameObject(gameObject);
        }
    }
}