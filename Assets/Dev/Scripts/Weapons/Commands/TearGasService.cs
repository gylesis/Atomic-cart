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
        private TickTimer _durationTimer;
        private TickTimer _damageTickTimer;

        public Subject<Unit> DurationEnded { get; } = new Subject<Unit>();
        
        public void ExplodeTearGas(Vector3 pos, TeamSide ownerTeam)
        {
            TearGasEffect tearGasEffect = FxController.Instance.SpawnEffectAt<TearGasEffect>("teargas", pos, destroyDelay: _duration);

            tearGasEffect.StartExpansion(_expansionRadius, _expansionTime + _effectHideDuration, (() =>
            {
                tearGasEffect.Hide(_effectHideDuration);
            }));
            
            _durationTimer = TickTimer.CreateFromSeconds(Runner ,_duration);

            Observable.Timer(TimeSpan.FromSeconds(_duration)).TakeUntilDestroy(this).Subscribe((l =>
            {
                DurationEnded.OnNext(Unit.Default);
                _tearGas.ToDestroy.OnNext(_tearGas);
                _tearGas = null;
            }));
            
            _damageTickTimer = TickTimer.CreateFromSeconds(Runner, _secondsDamageTick);

            _tearGas = Runner.Spawn(_tearGasPrefab, position: pos, onBeforeSpawned: ((runner, o) =>
            {
                TearGas gas = o.GetComponent<TearGas>();

                gas.ToDestroy.Take(1).Subscribe(OnToDestroy);
                gas.RPC_SetOwnerTeam(ownerTeam);
                gas.Init(Vector2.zero, 0, _damagePerSecond, Runner.LocalPlayer, _expansionRadius);
            }));
        }

        private void OnToDestroy(Projectile projectile)
        {
            Runner.Despawn(projectile.Object);
        }
        
        public override void FixedUpdateNetwork()
        {
            if(_durationTimer.ExpiredOrNotRunning(Runner)) return;

            if(_tearGas == null) return;
            
            if(_damageTickTimer.Expired(Runner))
            {
                _damageTickTimer = TickTimer.CreateFromSeconds(Runner, _secondsDamageTick);
            
                _tearGas.DealDamage();
            }
        }
    }
}