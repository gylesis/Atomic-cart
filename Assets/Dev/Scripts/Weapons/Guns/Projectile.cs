using System.Collections.Generic;
using Dev.Effects;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using Fusion.Addons.Physics;
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

        public float OverlapRadius => _overlapRadius;

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

        [Rpc]
        public void RPC_SetViewState(bool isEnabled)
        {
            _view.gameObject.SetActive(isEnabled);  
        }

        public override void FixedUpdateNetwork()
        {
            if (_collideWhileMoving == false) return;

            CheckCollisionsWhileMoving();
            
            if (HasStateAuthority == false) return;

            transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3)_moveDirection, Runner.DeltaTime * _force);
            //_networkRigidbody2D.Rigidbody.velocity = _moveDirection * _force * Runner.DeltaTime;
        }

        private void CheckCollisionsWhileMoving()
        {
            var overlapSphere = OverlapCircle(transform.position, _overlapRadius, _hitMask, out var colliders);

            if (overlapSphere)
            {
                PlayerRef shooter = Object.InputAuthority;

                bool needToDestroy = false;

                foreach (Collider2D collider in colliders)
                {
                    var isDamageable = collider.TryGetComponent<IDamageable>(out var damagable);

                    if (isDamageable)
                    {
                        var isPlayer = collider.TryGetComponent<PlayerCharacter>(out var player);

                        if (isPlayer)
                        {
                            PlayerRef target = player.Object.InputAuthority;

                            if (target == shooter) continue;

                            ApplyHitToPlayer(player);
                            needToDestroy = true;

                            break;
                        }
                        
                        if (damagable is IObstacleDamageable obstacleDamageable)
                        {
                            bool isStaticObstacle = damagable.Id == -1;

                            if (isStaticObstacle)
                            {
                                OnObstacleHit(damagable as Obstacle);
                            }

                            bool isObstacleWithHealth = damagable.Id == 0;

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

        protected virtual void ApplyHitToPlayer(PlayerCharacter playerCharacter)
        {
            ApplyDamage(playerCharacter, _owner, _damage);
        }

        protected void ApplyDamage(PlayerCharacter target, PlayerRef shooter, int damage)
        {
            PlayersHealthService.Instance.ApplyDamage(target.Object.InputAuthority, shooter, damage);
        }

        protected void ApplyDamage(PlayerRef target, PlayerRef shooter, int damage)
        {
            PlayersHealthService.Instance.ApplyDamage(target, shooter, damage);
        }

        protected bool OverlapCircle(Vector3 pos, float radius, LayerMask layerMask, out List<Collider2D> colliders)
        {
            Extensions.OverlapSphere(Runner, pos, radius, layerMask, out colliders);

            return colliders.Count > 0;
        }

        protected void ApplyForceToPlayer(PlayerCharacter playerCharacter, Vector2 forceDirection, float forcePower)
        {
            Debug.DrawRay(playerCharacter.transform.position, forceDirection * forcePower, Color.blue, 5f);

            playerCharacter.Rigidbody.AddForce(forceDirection * forcePower, ForceMode2D.Impulse);
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