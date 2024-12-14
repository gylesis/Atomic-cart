using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.Levels.Env
{
    public class Grass : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        
        private void Reset() => _renderer = GetComponent<Renderer>();

        private void Awake()
        {
            var speed = _renderer.material.GetFloat("_Speed");

            float percentValue = speed * 0.2f;
            
            _renderer.material.SetFloat("_Speed", Random.Range(speed - percentValue, speed + percentValue));
        }
    }
}