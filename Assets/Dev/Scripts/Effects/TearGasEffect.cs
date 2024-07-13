using System;
using DG.Tweening;
using Fusion;
using UnityEngine;

namespace Dev.Effects
{
    public class TearGasEffect : Effect
    {
        [SerializeField] private ParticleSystem _particleSystem;
        
        public void StartExpansion(float targetValue, float duration, Action onFinish = null)
        {
            transform.localScale = Vector3.one * 0.001f;

            transform.DOScale(targetValue, duration).OnComplete(() => onFinish?.Invoke());
        }

        public void Hide()
        {   
            RPC_Hide();
        }

        [Rpc]
        private void RPC_Hide()
        {
            Debug.Log($"Hide tear gas");
            ParticleSystem.MainModule mainModule = _particleSystem.main;
            mainModule.loop = false;
            mainModule.simulationSpeed = 3;
        }   
        
    }
}