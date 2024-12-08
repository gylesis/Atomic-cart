using UnityEngine;

namespace Dev
{
    public class MonoContext : MonoBehaviour
    {
        protected virtual void Awake() { }

        protected virtual void Start()
        {
            OnInjectCompleted();
        }

        protected virtual void OnInjectCompleted() { }
        
        protected virtual void Update() { }
    }
}