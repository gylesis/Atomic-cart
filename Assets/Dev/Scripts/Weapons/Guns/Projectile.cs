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

        
        [Networked] private Vector2 MoveDirection { get; set; }
        [Networked] private float Force { get; set; }
        [Networked] protected int Damage { get; set; }
        
        [Networked] protected SessionPlayer Owner { get; set; }
        public TeamSide OwnerTeamSide => Owner.TeamSide;
        

        private HealthObjectsService _healthObjectsService;
        protected HitsProcessor _hitsProcessor;
        private SessionStateService _sessionStateService;

        [Networked] public TickTimer DestroyTimer { get; set; }
        public Transform View => _view;
        public Subject<Projectile> ToDestroy { get; } = new Subject<Projectile>();
        public float OverlapRadius => _overlapRadius;
        private bool IsOwnerIsBot => Owner.IsBot;
        
        
        [Inject]
        private void Construct(HealthObjectsService healthObjectsService, HitsProcessor hitsProcessor, SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
            _hitsProcessor = hitsProcessor;
            _healthObjectsService = healthObjectsService;
        }

        [Rpc]
        public void RPC_SetOwner(SessionPlayer owner)
        {
            Owner = owner;
        }

        public void Init(Vector2 moveDirection, float force, int damage, SessionPlayer owner)
        {
            Owner = owner; 
            Damage = damage;
            Force = force;
            MoveDirection = moveDirection;

            transform.up = MoveDirection;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();

            _hitsProcessor.Hit.TakeUntilDestroy(this).Subscribe((OnHit));
        }

        private void OnHit(HitContext hitContext)
        {
            if (hitContext.DamagableType == DamagableType.Obstacle)
            {
                OnObstacleHit(hitContext.GameObject.GetComponent<Obstacle>());
            }

            if (hitContext.DamagableType == DamagableType.Player)
            {
                OnPlayerHit(hitContext.GameObject.GetComponent<PlayerCharacter>());
            }

            if (hitContext.DamagableType == DamagableType.Bot)
            {
                OnBotHit(hitContext.GameObject.GetComponent<Bot>());
            }
        }


        [Rpc]
        public void RPC_SetViewState(bool isEnabled)    
        {
            _view.gameObject.SetActive(isEnabled);
        }

        public override void FixedUpdateNetwork()
        {
            if (_checkOverlapWhileMoving == false) return;

            ProcessHitCollisionContext hitCollisionContext = new ProcessHitCollisionContext(Runner, this, transform.position,
                _overlapRadius, Damage, _hitMask, IsOwnerIsBot, false, Owner, OwnerTeamSide);

            _hitsProcessor.ProcessHitCollision(hitCollisionContext);

            if (HasStateAuthority == false) return;

            transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3)MoveDirection,
                Runner.DeltaTime * Force);
            //_networkRigidbody2D.Rigidbody.velocity = _moveDirection * _force * Runner.DeltaTime;
        }

        protected void ApplyDamageToUnit(NetworkObject target, PlayerRef shooter, int damage)
        {
            ApplyDamageContext damageContext = new ApplyDamageContext();
            damageContext.Damage = damage;
            damageContext.VictimObj = target;
            damageContext.Shooter = Owner;
            
            _healthObjectsService.ApplyDamage(damageContext);
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