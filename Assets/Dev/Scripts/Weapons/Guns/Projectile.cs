using System.Collections.Generic;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public abstract class Projectile : NetworkContext
    {
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] protected float _overlapRadius = 1f;
        [SerializeField] protected LayerMask _hitMask;

        [Networked] public TickTimer DestroyTimer { get; set; }

        public Subject<Projectile> ToDestroy { get; } = new Subject<Projectile>();

        private Vector3 _moveDirection;
        private float _force;
        protected int _damage;
        private PlayerRef _owner;

        public void Init(Vector3 moveDirection, float force, int damage, PlayerRef owner)
        {
            _owner = owner;
            _damage = damage;
            _force = force;
            _moveDirection = moveDirection;

            transform.up = _moveDirection;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            _networkRigidbody2D.Rigidbody.velocity = _moveDirection * _force * Runner.DeltaTime;

            var overlapSphere = OverlapSphere(transform.position, _overlapRadius, _hitMask, out var hits);


            if (overlapSphere)
            {
                foreach (LagCompensatedHit hit in hits)
                {
                    var isPlayer = hit.GameObject.TryGetComponent<Player>(out var player);

                    if (isPlayer)
                    {
                        PlayerRef owner = Object.InputAuthority;
                        PlayerRef target = player.Object.InputAuthority;

                        if (target == owner) continue;

                        ApplyHitToPlayer(player);

                        ToDestroy.OnNext(this);

                        break;
                    }
                    else
                    {
                        OnObstacleHit(hit);

                        ToDestroy.OnNext(this);

                        break;
                    }
                }
            }
        }

        protected virtual void OnObstacleHit(LagCompensatedHit obstacleHit) { }

        protected virtual void ApplyHitToPlayer(Player player)
        {
            ApplyDamage(player, _owner, _damage);
        }

        protected void ApplyDamage(Player target, PlayerRef shooter, int damage)
        {
            PlayersHealthService.Instance.ApplyDamage(target.Object.InputAuthority, shooter, damage);
        }
        
        protected void ApplyDamage(PlayerRef target, PlayerRef shooter, int damage)
        {
            PlayersHealthService.Instance.ApplyDamage(target, shooter, damage);
        }
        
        protected bool OverlapSphere(Vector3 pos, float radius, LayerMask layerMask, out List<LagCompensatedHit> hits)
        {
            hits = new List<LagCompensatedHit>();

            Runner.LagCompensation.OverlapSphere(pos, radius, Object.InputAuthority,
                hits, layerMask);

            return hits.Count > 0;
        }

        protected void ApplyForceToPlayer(Player player, Vector2 forceDirection, float forcePower)
        {
            Debug.DrawRay(player.transform.position, forceDirection * forcePower, Color.blue, 5f);

            player.Rigidbody.AddForce(forceDirection * forcePower, ForceMode2D.Impulse);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _overlapRadius);
        }
    }
}