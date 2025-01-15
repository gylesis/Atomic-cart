using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Weapons.Guns;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.Weapons.Commands
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
        private NetworkRunner _localNetRunner;

        public async UniTask CallMiniAirStrike(NetworkRunner networkRunner, Vector3 pos, SessionPlayer owner)
        {
            _localNetRunner = networkRunner;
            
            if (IsBusy) return;

            IsBusy = true;
            _markers.Clear();
            _miniAirStrikeExplosionBombCount = _miniAirStrikeBombCount;

            Vector3[] poses = new Vector3[_miniAirStrikeBombCount];

            float waitTimeForMarker = 0.05f;
            
            for (int index = 0; index < _miniAirStrikeBombCount; index++)
            {
                Vector3 spawnPos = pos + (Vector3)Random.insideUnitCircle * _miniAirStrikeRadius;
                poses[index] = spawnPos;
                
                SpawnMarker(index, spawnPos);
                
                await UniTask.Delay(TimeSpan.FromSeconds(waitTimeForMarker), cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(waitTimeForMarker * _miniAirStrikeBombCount / 2), cancellationToken: gameObject.GetCancellationTokenOnDestroy());

            for (int index = 0; index < _miniAirStrikeBombCount; index++)
            {
                Vector3 spawnPos = poses[index];

                SpawnAirStrikeBomb(index, spawnPos, owner, 1.5f).Forget();

                await UniTask.Delay(TimeSpan.FromSeconds(_cooldownBeforeNextBomb), cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            }
            
            
        }

        private void SpawnMarker(int index, Vector3 spawnPos)
        {
            AirStrikeMarker marker = _localNetRunner.Spawn(_airStrikerMarker, spawnPos);
            marker.Show();
            _markers.Add(index, marker);
        }

        private async UniTask SpawnAirStrikeBomb(int index, Vector3 spawnPos, SessionPlayer owner, float detonateDelay)
        {
            AirStrikeBomb bomb = _localNetRunner.Spawn(_airStrikeBombPrefab, spawnPos, null,  _localNetRunner.LocalPlayer, onBeforeSpawned: (runner, o) =>
            {
                DiInjecter.Instance.InjectGameObject(o.gameObject);

                AirStrikeBomb airStrikeBomb = o.GetComponent<AirStrikeBomb>();
                airStrikeBomb.RPC_SetOwner(owner);
                airStrikeBomb.Init(Vector2.zero, 0, _miniAirStrikeDamage, _explosionRadius);

                float size = Random.Range(0.8f, 1.2f);

                airStrikeBomb.transform.localScale = Vector3.one * 2;
                airStrikeBomb.View.localScale = Vector3.one * size;
            });

            DangerZoneViewProvider.Instance.SetDangerZoneView(transform.position, _explosionRadius, 1.5f);
            
            bomb.ToDestroy.Take(1).Subscribe((OnToDestroy));
            bomb.transform.DOScale(1, 0.8f);

            await UniTask.Delay(TimeSpan.FromSeconds(detonateDelay), cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            
            _markers[index].Hide();
            _markers.Remove(index);
            bomb.Detonate();
        }

        private void OnToDestroy(Projectile projectile)
        {
            _miniAirStrikeExplosionBombCount--;
            _localNetRunner.Despawn(projectile.Object);

            if (_miniAirStrikeExplosionBombCount <= 0)
            {
                _miniAirStrikeExplosionBombCount = 0;
                AirstrikeCompleted.OnNext(Unit.Default);
                IsBusy = false;
            }
        }
    }
}