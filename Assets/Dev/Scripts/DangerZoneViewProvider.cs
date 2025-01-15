using System;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;

namespace Dev
{
    public class DangerZoneViewProvider : NetSingleton<DangerZoneViewProvider>
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private DangerZoneView _prefab;
        
        private ObjectPool<DangerZoneView> _viewsPool;

        protected override void Awake()
        {
            base.Awake();
            _viewsPool = new ObjectPool<DangerZoneView>(CreateFunc, ActionOnGet, ActionOnRelease, null, true, 25);
        }

        private void ActionOnRelease(DangerZoneView obj)
        {
            obj.gameObject.SetActive(false);
            
            obj.transform.localScale = Vector3.one;
            obj.transform.rotation = Quaternion.identity;
        }

        private void ActionOnGet(DangerZoneView obj)
        {
            obj.gameObject.SetActive(true);
        }

        private DangerZoneView CreateFunc()
        {
            DangerZoneView dangerZoneView = Instantiate(_prefab, _parent);
            return dangerZoneView;
        }

        public void SetDangerZoneView(Vector3 position, float dangerRadius, float hideDelay = 5, bool isLocal = false)
        {
            if(isLocal)
                SetDangerZoneViewInternal(position, dangerRadius, hideDelay);
            else
                RPC_SetDangerZoneView(position, dangerRadius, hideDelay);
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        private void RPC_SetDangerZoneView(Vector3 position, float dangerRadius, float hideDelay = 5)
        {
            SetDangerZoneViewInternal(position, dangerRadius, hideDelay);
        }

        private void SetDangerZoneViewInternal(Vector3 position, float dangerRadius, float hideDelay = 5)
        {
            var dangerZoneView = _viewsPool.Get();
            
            dangerZoneView.transform.position = position;
            dangerZoneView.SetRadius(dangerRadius);

            Observable.Timer(TimeSpan.FromSeconds(hideDelay)).Subscribe(_ =>
            {
                _viewsPool.Release(dangerZoneView);
            }).AddTo(this);
        }
        
    }
}