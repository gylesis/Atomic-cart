using System.Collections.Generic;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Utils;
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
        [SerializeField] private bool _needToRenderCollisionPrediction = true;

        [SerializeField] protected Transform _view;
        [SerializeField] protected NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] protected float _overlapRadius = 1f;

        protected HitsProcessor _hitsProcessor;

        /// <summary>
        /// Do projectile need to register collision while flying?
        /// </summary>
        protected abstract bool CheckForHitsWhileMoving { get; }

        [Networked] private Vector2 MoveDirection { get; set; }
        [Networked] private float Force { get; set; }
        [Networked] protected int Damage { get; set; }
        [Networked] protected SessionPlayer Owner { get; set; }
        [Networked] public TickTimer DestroyTimer { get; set; }
        [Networked] public NetworkBool IsAlive { get; set; } = true;

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

            _hitsProcessor.Hit.Subscribe(OnHit).AddTo(this);
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_SetOwner(SessionPlayer owner)
        {
            Owner = owner;
        }

        public void Init(Vector2 moveDirection, float force, int damage)
        {
            Damage = damage;
            Force = force;
            MoveDirection = moveDirection;

            transform.up = MoveDirection;
        }

        private void OnHit(HitContext hitContext)
        {
            if(Object.IsValid == false) return;
            
            if (hitContext.DamagableType is DamagableType.Obstacle or DamagableType.ObstacleWithHealth)
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

        public void SetViewStateLocal(bool isEnabled)
        {
            _view.gameObject.SetActive(isEnabled);
        }

        public override void FixedUpdateNetwork()
        {
            if (Object == null) return;

            if (IsAlive == false) return;

            if (HasStateAuthority == false) return;
            
            if (CheckForHitsWhileMoving)
            {
                ProcessHitCollisionContext hitCollisionContext = new ProcessHitCollisionContext(Object.Id, transform.position, _overlapRadius, Damage, false, Owner);

                _hitsProcessor.ProcessHit(hitCollisionContext);
            }

            transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3)MoveDirection,
                Runner.DeltaTime * Force);
        }

        public override void Render()
        {
            base.Render();

            RenderBulletCollisionOnProxies();
        }

        private void RenderBulletCollisionOnProxies()
        {
            if (IsProxy == false) return;

            if (_needToRenderCollisionPrediction == false) return;

            bool hitSomething = false;

            if (Owner.IsBot)
                hitSomething = _hitsProcessor.ProcessCollision(Runner, transform.position, _overlapRadius, Owner.Id);
            else
                hitSomething = _hitsProcessor.ProcessCollision(Runner, transform.position, _overlapRadius, Owner.Owner);

            if (hitSomething)
                SetViewStateLocal(false);
        }

        protected virtual void OnObstacleHit(Obstacle obstacle)
        {
            PlayShakeEffect();
            PlayDefaultHitSound(obstacle.transform.position);
        }

        protected virtual void OnPlayerHit(PlayerCharacter playerCharacter)
        {
            PlayShakeEffect();   
            PlayDefaultHitSound(playerCharacter.transform.position);
        }

        protected virtual void OnBotHit(Bot bot)
        {
            PlayShakeEffect();
            PlayDefaultHitSound(bot.transform.position);
        }

        protected virtual void OnDummyHit(DummyTarget dummyTarget)
        {
            PlayShakeEffect();
            PlayDefaultHitSound(dummyTarget.transform.position);
        }

        protected virtual void PlayShakeEffect() // TODO think, if it really need to feel impact when projectile hit smthg
        {
            //CameraService.Instance.ShakeIfNeed(transform.position, "bullet", SessionPlayer.Default);
        }

        protected virtual void PlayDefaultHitSound(Vector3 position)
        {
            RPC_PlaySound("ak_hit", position, 40);
        }
           
        [Rpc(Channel = RpcChannel.Reliable)]
        protected void RPC_PlaySound(NetworkString<_16> soundType, Vector3 at, float radius)
        {
            //SoundController.Instance.PlaySoundAt(soundType.ToString(), at, radius);
        }
        
        protected virtual void OnDrawGizmosSelected() { }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _overlapRadius);
        }
    }


    public class Recordable : NetworkContext
    {
        [SerializeField] private int _recordSize = 120;
        
        private Queue<Vector3> _positions = new Queue<Vector3>();
        
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
        
            if(_positions.Count >= _recordSize)
                _positions.Dequeue();
            
            _positions.Enqueue(transform.position);
        }
    }
    
}