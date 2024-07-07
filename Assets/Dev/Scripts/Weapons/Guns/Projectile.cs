using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Fusion;
using Fusion.Addons.Physics;
using UniRx;
using UnityEngine;
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
        protected abstract bool CheckForHitsWhileMoving { get; }
        
        protected HitsProcessor _hitsProcessor;
        
        [Networked] private Vector2 MoveDirection { get; set; }
        [Networked] private float Force { get; set; }
        [Networked] protected int Damage { get; set; }
        [Networked] protected SessionPlayer Owner { get; set; }
        [Networked] public TickTimer DestroyTimer { get; set; }
        
        public Transform View => _view;
        public Subject<Projectile> ToDestroy { get; } = new Subject<Projectile>();
        public float OverlapRadius => _overlapRadius;


        [Inject]
        private void Construct(HitsProcessor hitsProcessor)
        {   
            _hitsProcessor = hitsProcessor;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();

            _hitsProcessor.Hit.TakeUntilDestroy(this).Subscribe((OnHit));
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

        private void OnHit(HitContext hitContext)
        {
            if (hitContext.DamagableType == DamagableType.Obstacle || hitContext.DamagableType == DamagableType.ObstacleWithHealth)
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

            if (hitContext.DamagableType == DamagableType.DummyTarget)
            {
                OnDummyHit(hitContext.GameObject.GetComponent<DummyTarget>());
            }
        }


        [Rpc]
        public void RPC_SetViewState(bool isEnabled)    
        {
            _view.gameObject.SetActive(isEnabled);
        }

        public override void FixedUpdateNetwork()
        {
            if (CheckForHitsWhileMoving)
            {
                ProcessHitCollisionContext hitCollisionContext = new ProcessHitCollisionContext(Runner, this, transform.position, _overlapRadius, Damage, _hitMask, false, Owner);

                _hitsProcessor.ProcessHitCollision(hitCollisionContext);
            }

            if (HasStateAuthority == false) return;

            transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3)MoveDirection,
                Runner.DeltaTime * Force);
            //_networkRigidbody2D.Rigidbody.velocity = _moveDirection * _force * Runner.DeltaTime;
        }

        protected virtual void OnObstacleHit(Obstacle obstacle) { }

        protected virtual void OnPlayerHit(PlayerCharacter playerCharacter) { }

        protected virtual void OnBotHit(Bot bot) { }
        
        protected virtual void OnDummyHit(DummyTarget dummyTarget) { }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _overlapRadius);
        }
    }
}