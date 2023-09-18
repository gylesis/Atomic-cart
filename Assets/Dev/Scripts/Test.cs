using DG.Tweening;
using UnityEngine;

namespace Dev
{
    [ExecuteInEditMode]
    public class Test : MonoBehaviour
    {

        [SerializeField] private float _targetValue = 6;
        [SerializeField] private float _duration;

        [SerializeField] private Ease _ease = Ease.Linear;
        
        [ContextMenu("Explosion")]
        private void Epxl()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(_targetValue, _duration).SetEase(_ease);
        }
        
    }
}