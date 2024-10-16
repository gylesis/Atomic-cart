using System;
using Dev.BotsLogic;
using Dev.Effects;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons.Guns;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dev.Weapons
{
    public class Landmine : ExplosiveProjectile
    {
        protected override bool CheckForHitsWhileMoving => false;
        
        [SerializeField] private float _detonateSeconds;
        [SerializeField] private float _searchRadius = 5;
        [SerializeField] private float _detonateRadius = 5;

        [FormerlySerializedAs("_detonateCircle")] [SerializeField] private SpriteRenderer _detonateCircleSprite;
        
        private TeamSide _ownerTeam;

        private bool _hasAnyTarget;

        private TickTimer _detonateTimer;

        private bool _exploding;


        protected override void LoadLateInjection()
        {
            base.LoadLateInjection();
            _detonateCircleSprite.transform.localScale = new Vector3(_detonateRadius,_detonateRadius,1);
        }
        
        public override void FixedUpdateNetwork()
        {
            if(HasStateAuthority == false) return;

            if(_exploding) return;
            
            if (_detonateTimer.Expired(Runner))
            {
                _detonateTimer = TickTimer.None;
                Explode();
            }
            
            if(_hasAnyTarget) return;
            
            ProcessExplodeContext explodeContext = new ProcessExplodeContext(Owner, _detonateRadius, -1, transform.position, false);
            
            _hitsProcessor.ProcessExplodeAndHitUnits(explodeContext, Exploded); // TODO remove Exploded callback and make other method for simulating explode

            void Exploded(NetworkObject victim, SessionPlayer owner, DamagableType damagableType, int damage, bool isDamageFromServer)
            {
                switch (damagableType)
                {
                    case DamagableType.Bot:
                    case DamagableType.Player:
                        StartDetonation();
                        _hasAnyTarget = true;
                        break;
                    case DamagableType.ObstacleWithHealth:
                    case DamagableType.Obstacle:
                    case DamagableType.DummyTarget:
                    default:
                        break;
                }
            }

        }

        private void Explode()
        {
            ExplodeAndDealDamage(_detonateRadius);
            _exploding = true;

            _view.DOScale(0, 0.5f);
            
            Extensions.Delay(0.5f, destroyCancellationToken, () => ToDestroy.OnNext(this));
        }

        protected override void OnExplode(HitContext context)
        {
            FxController.Instance.SpawnEffectAt<Effect>("landmine_explosion", transform.position);
            base.OnExplode(context);
        }

        private void StartDetonation()
        {
            Sequence sequence = DOTween.Sequence();

            Color originColor = _detonateCircleSprite.color;

            Color targetColor = originColor;
            targetColor.a = 0.8f;

            float duration = 0.3f;
            
            sequence.Append(_detonateCircleSprite.DOColor(targetColor, duration));
            sequence.Append(_detonateCircleSprite.DOColor(originColor, duration));
            sequence.SetLoops(4);

            sequence.Play();
            
            _detonateTimer = TickTimer.CreateFromSeconds(Runner, _detonateSeconds);
        }
        
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            var position = transform.position;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(position, _searchRadius);
            Handles.Label(position + Vector3.up * _searchRadius, "Search radius");
        }
#endif
    }
}