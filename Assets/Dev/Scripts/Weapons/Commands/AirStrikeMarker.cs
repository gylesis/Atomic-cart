using Dev.Infrastructure.Networking;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.Weapons.Commands
{
    public class AirStrikeMarker : NetworkContext
    {
        [SerializeField] private Transform _view;

        [SerializeField] private float _rotateSpeed = 1.2f;

        public Transform View => _view;

        public void Show()
        {
            _view.rotation = Quaternion.Euler(0,0,Random.Range(0, 360f)); 
            
            //transform.localScale = Vector3.one *  0.001f;
            _view.DOScale(1, 1f);
        }

        public void Hide()
        {
            _view.DOScale(0, 1).OnComplete((() =>
            {
                Runner.Despawn(Object);
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