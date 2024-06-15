using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Weapons.Guns;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.Weapons
{
    public class AirStrikeController : NetworkContext
    {
        [SerializeField] private AirStrikeBomb _airStrikeBombPrefab;
        [SerializeField] private AirStrikeMarker _airStrikerMarker;

        [SerializeField] private int _miniAirStrikeBombCount = 10;
        [SerializeField] private float _miniAirStrikeRadius = 15;
        [SerializeField] private float _cooldownBeforeNextBomb = 0.5f;

        [SerializeField] private float _explosionRadius;
        
        [SerializeField] private int _miniAirStrikeDamage = 50;
        
        
        public Subject<Unit> AirstrikeCompleted { get; } = new Subject<Unit>();

        [Networked] public NetworkBool IsBusy { get; private set; }

        private int _miniAirStrikeExplosionBombCount = 0;

        private Dictionary<int, AirStrikeMarker> _markers = new Dictionary<int, AirStrikeMarker>();

        public async void CallMiniAirStrike(Vector3 pos, TeamSide ownerTeam)
        {
            if (IsBusy) return;

            IsBusy = true;

            _miniAirStrikeExplosionBombCount = _miniAirStrikeBombCount;

            Vector3[] poses = new Vector3[_miniAirStrikeBombCount];
            
            for (int index = 0; index < _miniAirStrikeBombCount; index++)
            {
                Vector3 spawnPos = pos + (Vector3)Random.insideUnitCircle * _miniAirStrikeRadius;
                poses[index] = spawnPos;
                
                SpawnMarker(index, spawnPos);
                
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }

            for (int index = 0; index < _miniAirStrikeBombCount; index++)
            {
                Vector3 spawnPos = poses[index];

                SpawnAirStrikeBomb(index, spawnPos, ownerTeam, 1.5f);

                await UniTask.Delay(TimeSpan.FromSeconds(_cooldownBeforeNextBomb));
            }
        }

        private void SpawnMarker(int index, Vector3 spawnPos)
        {
            AirStrikeMarker marker = Runner.Spawn(_airStrikerMarker, spawnPos);
            marker.Show();
            _markers.Add(index, marker);
        }

        private void SpawnAirStrikeBomb(int index, Vector3 spawnPos, TeamSide ownerTeam, float detonateDelay)
        {
            AirStrikeBomb bomb = Runner.Spawn(_airStrikeBombPrefab, spawnPos, onBeforeSpawned: (runner, o) =>
            {
                DependenciesContainer.Instance.Inject(o.gameObject);

                AirStrikeBomb airStrikeBomb = o.GetComponent<AirStrikeBomb>();
                airStrikeBomb.RPC_SetOwnerTeam(ownerTeam);
                airStrikeBomb.Init(Vector2.zero, 0, _miniAirStrikeDamage, Runner.LocalPlayer, _explosionRadius);

                float size = Random.Range(0.8f, 1.2f);

                airStrikeBomb.transform.localScale = Vector3.one * 2;
                airStrikeBomb.View.localScale = Vector3.one * size;
            });

            bomb.ToDestroy.Take(1).Subscribe((OnToDestroy));
            bomb.transform.DOScale(1, 0.8f);

            Observable.Timer(TimeSpan.FromSeconds(detonateDelay)).TakeUntilDestroy(this).Subscribe((l =>
            {
                _markers[index].Hide();
                _markers.Remove(index);
                bomb.StartDetonate();
            }));
        }

        private void OnToDestroy(Projectile projectile)
        {
            _miniAirStrikeExplosionBombCount--;
            Runner.Despawn(projectile.Object);

            if (_miniAirStrikeExplosionBombCount <= 0)
            {
                _miniAirStrikeExplosionBombCount = 0;
                AirstrikeCompleted.OnNext(Unit.Default);
                IsBusy = false;
            }
        }
    }
}