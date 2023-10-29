using System.Collections.Generic;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public abstract class Projectile : NetworkContext
    {
        [SerializeField] private Transform _view;
        [SerializeField] protected NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] protected float _overlapRadius = 1f;
        [SerializeField] protected LayerMask _hitMask;

        /// <summary>
        /// Do projectile need to register collision while flying?
        /// </summary>
        [SerializeField] private bool _collideWhileMoving;
        
        [Networked] public TickTimer DestroyTimer { get; set; }

        public Transform View => _view;

        public Subject<Projectile> ToDestroy { get; } = new Subject<Projectile>();

        private Vector2 _moveDirection;
        private float _force;
        protected int _damage;
        private PlayerRef _owner;

        public void Init(Vector2 moveDirection, float force, int damage, PlayerRef owner)
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

            if(_collideWhileMoving == false) return;
            
            CheckCollisionsWhileMoving();
        }

        private void CheckCollisionsWhileMoving()
        {
            var overlapSphere = OverlapSphere(transform.position, _overlapRadius, _hitMask, out var hits);

            if (overlapSphere)
            {
                PlayerRef shooter = Object.InputAuthority;

                bool needToDestroy = false;

                foreach (LagCompensatedHit hit in hits)
                {
                    var isDamageable = hit.GameObject.TryGetComponent<IDamageable>(out var damagable);

                    if (isDamageable == false || damagable is IObstacleDamageable obstacleDamageable)
                    {
                        bool isStaticObstacle = damagable.Id == -1;

                        bool isObstacleWithHealth = damagable.Id == 0;

                        if (isStaticObstacle)
                        {
                            OnObstacleHit(damagable as Obstacle);
                        }

                        if (isObstacleWithHealth)
                        {
                            OnObstacleHit(damagable as Obstacle);

                            ApplyDamageToObstacle(damagable as ObstacleWithHealth, shooter, _damage);
                        }

                        needToDestroy = true;
                        break;
                    }

                    bool isDummyTarget = damagable.Id == -2;

                    if (isDummyTarget)
                    {
                        DummyTarget dummyTarget = damagable as DummyTarget;

                        ApplyDamageToDummyTarget(dummyTarget, shooter, _damage);
                        needToDestroy = true;

                        break;
                    }

                    var isPlayer = hit.GameObject.TryGetComponent<Player>(out var player);

                    if (isPlayer)
                    {
                        PlayerRef target = player.Object.InputAuthority;

                        if (target == shooter) continue;

                        ApplyHitToPlayer(player);
                        needToDestroy = true;

                        break;
                    }
                }

                if (needToDestroy)
                {
                    ToDestroy.OnNext(this);
                }
            }
        }

        protected virtual void OnObstacleHit(Obstacle obstacle) { }

        protected void ApplyDamageToDummyTarget(DummyTarget dummyTarget, PlayerRef shooter, int damage)
        {
            PlayersHealthService.Instance.ApplyDamageToDummyTarget(dummyTarget, shooter, damage);
        }

        protected void ApplyDamageToObstacle(ObstacleWithHealth obstacleWithHealth, PlayerRef shooter, int damage)
        {
            ObstaclesManager.Instance.ApplyDamageToObstacle(shooter, obstacleWithHealth, damage);
        }

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
            Extensions.OverlapSphere(Runner, pos, radius, layerMask, out hits);
            
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

    public interface IDamageable
    {
        int Id { get; }
    }
}