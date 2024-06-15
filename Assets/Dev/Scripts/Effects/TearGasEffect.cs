using System;
using DG.Tweening;
using UnityEngine;

namespace Dev.Effects
{
    public class TearGasEffect : Effect
    {
        public void StartExpansion(float targetValue, float duration, Action onFinish = null)
        {
            transform.localScale = Vector3.one * 0.001f;

            transform.DOScale(targetValue, duration).OnComplete(() => onFinish?.Invoke());
        }

        public void Hide(float duration)
        {
            transform.DOScale(0, duration);
        }
        
    }
}