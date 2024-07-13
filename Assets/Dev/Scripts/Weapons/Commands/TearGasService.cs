using System;
using Dev.Effects;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Weapons.Guns;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public class TearGasService : NetworkContext
    {
        [SerializeField] private TearGas _tearGasPrefab;
        [SerializeField] private float _expansionTime = 3;
        [SerializeField] private float _expansionRadius = 10;
        [SerializeField] private int _damagePerSecond = 25;
        [SerializeField] private float _duration = 10;

        [SerializeField] private float _effectHideDuration = 1;
        
        /// <summary>
        /// Hello
        /// </summary>
        [SerializeField] private float _secondsDamageTick = 1.5f;
            
        private TearGas _tearGas;
        [Networked] private TickTimer _durationTimer { get; set; }
        [Networked] private TickTimer _damageTickTimer{ get; set; }
        
        private NetworkRunner _localNetRunner;

        public Subject<Unit> DurationEnded { get; } = new Subject<Unit>();
        
        public void ExplodeTearGas(NetworkRunner networkRunner, Vector3 pos, SessionPlayer owner)
        {
            _localNetRunner = networkRunner;

            TearGasEffect tearGasEffect = FxController.Instance.SpawnEffectAt<TearGasEffect>("teargas", pos, destroyDelay: _duration);

            tearGasEffect.StartExpansion(_expansionRadius, _expansionTime + _effectHideDuration, (() =>
            {   
                tearGasEffect.Hide(_effectHideDuration);
            }));
            
            _durationTimer = TickTimer.CreateFromSeconds(_localNetRunner ,_duration);

            Observable.Timer(TimeSpan.FromSeconds(_duration)).TakeUntilDestroy(this).Subscribe((l =>
            {
                DurationEnded.OnNext(Unit.Default);
                _tearGas.ToDestroy.OnNext(_tearGas);
                _tearGas = null;
            }));
            
            _damageTickTimer = TickTimer.CreateFromSeconds(_localNetRunner, _secondsDamageTick);

            _tearGas = _localNetRunner.Spawn(_tearGasPrefab, position: pos, onBeforeSpawned: ((runner, o) =>
            {
                TearGas gas = o.GetComponent<TearGas>();

                gas.ToDestroy.Take(1).Subscribe(OnToDestroy);
                gas.RPC_SetOwner(owner);
                gas.Init(Vector2.zero, 0, _damagePerSecond, _expansionRadius);
            }));
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