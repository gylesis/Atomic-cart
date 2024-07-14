using Dev.Infrastructure;
using Dev.Utils;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class GrenadeProjectile : ExplosiveProjectile
    {
        protected override bool CheckForHitsWhileMoving => false;

        private Vector2 _originPos;
        private Vector2 _targetPos;
        private TickTimer DetonateTimer;
        private float _detonateTime;

        [Networked] private float FlyTargetTime { get; set; }
        [Networked] private TickTimer FlyTimer { get; set; }
        [Networked] private Vector3 TargetSize { get; set; }
        [Networked] private Vector3 OriginSize { get; set; }


        public void Init(Vector3 moveDirection, float force, int damage, float explosionRadius, Vector2 targetPos,
                         float flyTime, float detonateTime)
        {
            _detonateTime = detonateTime;
            FlyTargetTime = flyTime;
            _originPos = transform.position;
            _targetPos = targetPos;

            FlyTimer = TickTimer.CreateFromSeconds(Runner, FlyTargetTime);

            TargetSize = View.localScale * 2;
            OriginSize = View.localScale;

            Init(moveDirection, force, damage, explosionRadius);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            if (DetonateTimer.Expired(Runner))
            {
                DetonateTimer = TickTimer.None;
                ExplodeAndDealDamage(_explosionRadius);
                ToDestroy.OnNext(this);
                return;
            }
    
            if (FlyTimer.IsRunning == false) return;

            if (FlyTimer.Expired(Runner))
            {
                FlyTimer = TickTimer.None;
                DetonateTimer = TickTimer.CreateFromSeconds(Runner, _detonateTime);
                return;
            }

            MoveAlongDirection();
        }

        private void MoveAlongDirection()
        {
            if (FlyTimer.IsRunning == false) return;

            float remainingTime = FlyTimer.RemainingTime(Runner).Value;

            float t = 1 - (remainingTime / FlyTargetTime);
            float value = GameSettingProvider.GameSettings.GrenadeFlyFunction.Evaluate(t);

            Vector2 pos = Vector2.Lerp(_originPos, _targetPos, value);

            transform.position = pos;
        }

        public override void Render()
        {
            if (FlyTimer.IsRunning == false) return;

            float remainingTime = FlyTimer.RemainingTime(Runner).Value;

            float t = 1 - (remainingTime / FlyTargetTime);

            float sizeValue = GameSettingProvider.GameSettings.GrenadeFlySizeFunction.Evaluate(t);

            View.localScale = Vector3.Lerp(OriginSize, TargetSize, sizeValue);
        }
    }
}