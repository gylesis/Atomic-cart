using System;
using Dev.Infrastructure;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.Weapons
{
    public class AirStrikeMarker : NetworkContext
    {
        [SerializeField] private Transform _view;

        [SerializeField] private float _rotateSpeed = 1.2f;

        public Transform View => _view;

        public void Animate()
        {
            _view.transform.rotation = Quaternion.Euler(0,0,Random.Range(0, 360f)); 
            
            Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe((l =>
            {
                _view.DOScale(0, 0.5f).OnComplete((() => { Runner.Despawn(Object); }));
            }));
        }

        public override void Render()
        {
            Vector3 euler = _view.transform.rotation.eulerAngles;
            euler.z += Time.deltaTime * _rotateSpeed;
            _view.transform.rotation = Quaternion.Euler(euler);
        }

    }
}