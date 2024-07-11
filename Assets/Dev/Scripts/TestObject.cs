using System;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

namespace Dev
{
    public class TestObject : NetworkBehaviour
    {
        [Networked, SerializeField] private float _value2 { get; set; }
        
        public void Setup(float value)
        {
            _value2 = value;
        }

        public override void Spawned()
        {
        }

        private void Awake()
        {
           // Debug.Log($"Value {_value2}", gameObject);
        }
    }
}