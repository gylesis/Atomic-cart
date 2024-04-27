using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Fusion;
using Fusion.Addons.Physics;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Dev.Weapons.Guns
{
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public abstract class Projectile : NetworkContext
    {
        [SerializeField] protected Transform _view;
        [SerializeField] protected NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] protected float _overlapRadius = 1f;
        [SerializeField] protected LayerMask _hitMask;

        /// <summary>
        /// Do projectile need to register collision while flying?
        /// </summary>
        [FormerlySerializedAs("_collideWhileMoving")] [SerializeField]
        private bool _checkOverlapWhileMoving;

        [Networked] public TickTimer DestroyTimer { get; set; }

        public Transform View => _view;

        public Subject<Projectile> ToDestroy { get; } = new Subject<Projectile>();

        public float OverlapRadius => _overlapRadius;

        private Vector2 _moveDirection;
        private float _force;
        protected int _damage;
        private PlayerRef _owner;

        private HealthObjectsService _healthObjectsService;
        private HitsProcessor _hitsProcessor;

        private bool _isOwnerIsBot => _owner == PlayerRef.None;

        [Inject]
        private void Construct(HealthObjectsService healthObjectsService, HitsProcessor hitsProcessor)
        {
            _hitsProcessor = hitsProcessor;
            _healthObjectsService = healthObjectsService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();

            _hitsProcessor.Hit.TakeUntilDestroy(this).Subscribe((OnHit));
        }

        private void OnHit(HitContext hitContext)
        {
            if (hitContext.HitType == HitType.Obstacle)
            {
                OnObstacleHit(hitContext.GameObject.GetComponent<Obstacle>());
            }

            if (hitContext.HitType == HitType.Player)
            {
                OnPlayerHit(hitContext.GameObject.GetComponent<PlayerCharacter>());
            }

            if (hitContext.HitType == HitType.Bot)
            {
                OnBotHit(hitContext.GameObject.GetComponent<Bot>());
            }
        }

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
            if (_checkOverlapWhileMoving == false) return;

            ProcessCollisionContext collisionContext = new ProcessCollisionContext(Runner, this, transform.position,
                _overlapRadius, _damage, _hitMask, _isOwnerIsBot);

            _hitsProcessor.ProcessCollision(collisionContext);

            if (HasStateAuthority == false) return;

            transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3)_moveDirection,
                Runner.DeltaTime * _force);
            //_networkRigidbody2D.Rigidbody.velocity = _moveDirection * _force * Runner.DeltaTime;
        }

        protected void ApplyDamage(NetworkObject target, PlayerRef shooter, int damage)
        {
            _healthObjectsService.ApplyDamage(target, shooter, damage);
        }

        protected void ApplyDamageToObstacle(ObstacleWithHealth obstacleWithHealth, PlayerRef shooter, int damage)
        {
            ObstaclesManager.Instance.ApplyDamageToObstacle(shooter, obstacleWithHealth, damage);
        }

        protected virtual void OnObstacleHit(Obstacle obstacle) { }

        protected virtual void OnPlayerHit(PlayerCharacter playerCharacter) { }

        protected virtual void OnBotHit(Bot bot) { }

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
}