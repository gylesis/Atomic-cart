using System;
using Cysharp.Threading.Tasks;
using Dev.Effects;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Utils;
using Dev.Weapons.Guns;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons.Commands
{
    public class TearGasService : NetworkContext
    {
        [SerializeField] private TearGas _tearGasPrefab;
        [SerializeField] private float _expansionTime = 3;
        [SerializeField] private float _expansionRadius = 10;
        [SerializeField] private int _damagePerSecond = 25;
        [SerializeField] private float _duration = 10;
        [SerializeField] private float _flyDuration = 0.5f;
        
        /// <summary>
        /// Hello
        /// </summary>
        [SerializeField] private float _secondsDamageTick = 1.5f;
            
        private TearGas _tearGas;
        [Networked] private TickTimer _durationTimer { get; set; }
        [Networked] private TickTimer _damageTickTimer{ get; set; }
        
        private NetworkRunner _localNetRunner;

        public Subject<Unit> DurationEnded { get; } = new Subject<Unit>();
        
        public async void ExplodeTearGas(NetworkRunner networkRunner, Vector3 origin, Vector3 targetPos, SessionPlayer owner)
        {
            _localNetRunner = networkRunner;

            _tearGas = _localNetRunner.Spawn(_tearGasPrefab, position: origin, onBeforeSpawned: ((runner, o) =>
            {
                TearGas gas = o.GetComponent<TearGas>();

                gas.ToDestroy.Take(1).Subscribe(OnToDestroy);
                gas.RPC_SetOwner(owner);
                gas.Init(Vector2.zero, 0, _damagePerSecond, _expansionRadius);
            }));
            
            AnimateFly(targetPos);
            
            await UniTask.Delay(TimeSpan.FromSeconds(_flyDuration),
                cancellationToken: destroyCancellationToken);
            
            TearGasEffect tearGasEffect = FxController.Instance.SpawnEffectAt<TearGasEffect>("teargas", targetPos, destroyDelay: _duration);

            tearGasEffect.StartExpansion(_expansionRadius, (_duration + 1) * 0.5f, (() =>
            {   
                tearGasEffect.Hide();
            }));
            
            _durationTimer = TickTimer.CreateFromSeconds(_localNetRunner ,_duration);

            Extensions.Delay(_duration, destroyCancellationToken, () =>
            {
                DurationEnded.OnNext(Unit.Default);
                _tearGas.ToDestroy.OnNext(_tearGas);
                _tearGas = null;
            });
            
            _damageTickTimer = TickTimer.CreateFromSeconds(_localNetRunner, _secondsDamageTick);
        }

        private void AnimateFly(Vector3 targetPos)
        {
            _tearGas.transform.DOMove(targetPos, _flyDuration);
            _tearGas.transform.DOScale(Vector3.one * 1.2f, _flyDuration / 2).SetEase(Ease.InSine).OnComplete((() => _tearGas.transform.DOScale(Vector3.one, _flyDuration / 2).SetEase(Ease.OutSine)));
        }

        private void OnToDestroy(Projectile projectile)
        {
            projectile.RPC_DoScale(1, 0);
            
            _localNetRunner.Despawn(projectile.Object);
        }

        public override void Render()
        {
            if(_localNetRunner == null) return;
            
            if(_durationTimer.ExpiredOrNotRunning(_localNetRunner)) return;

            if(_tearGas == null) return;
            
            if(_damageTickTimer.Expired(_localNetRunner))
            {
                _damageTickTimer = TickTimer.CreateFromSeconds(_localNetRunner, _secondsDamageTick);
                
                _tearGas.DealDamage();
            }
        }
    }
}