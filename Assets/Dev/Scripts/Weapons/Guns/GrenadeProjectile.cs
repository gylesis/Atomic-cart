using Dev.Infrastructure;
using Dev.Utils;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class GrenadeProjectile : ExplosiveProjectile
    {
        [Networked] private float FlyTargetTime { get; set; }
        
        private Vector2 _originPos;
        private Vector2 _targetPos;

        [Networked] private TickTimer FlyTimer { get; set; }

        private bool _hasFlied;
      
        
        [Networked] private Vector3 TargetSize {get; set;}
        [Networked] private Vector3 OriginSize {get; set;}

        private TickTimer DetonateTimer;
            
        public void Init(Vector3 moveDirection, float force, int damage, PlayerRef owner, float explosionRadius, Vector2 targetPos, float flyTime)
        {
            FlyTargetTime = flyTime;
            _originPos = transform.position;
            _targetPos = targetPos;
            
            FlyTimer = TickTimer.CreateFromSeconds(Runner, FlyTargetTime);
            
            TargetSize = View.localScale * 2;
            OriginSize = View.localScale;
            
            Init(moveDirection, force, damage, owner, explosionRadius);
        }
        
        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            if (DetonateTimer.Expired(Runner))
            {
                ExplodeAndHitPlayers(_explosionRadius);
                ToDestroy.OnNext(this);
                return;
            }
            
            if(FlyTimer.IsRunning == false) return;
            
            if(_hasFlied) return;

            if (FlyTimer.Expired(Runner))
            {
                _hasFlied = true;
                DetonateTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);
                return;
            }

            MoveAlongDirection();
        }

        private void MoveAlongDirection()
        {
            if(FlyTimer.IsRunning == false) return;
            
            float remainingTime = FlyTimer.RemainingTime(Runner).Value;

            float t = 1 - (remainingTime / FlyTargetTime);
            float value = GameSettingProvider.GameSettings.GrenadeFlyFunction.Evaluate(t);

            Vector2 pos = Vector2.Lerp(_originPos, _targetPos, value);

            _networkRigidbody2D.Rigidbody.MovePosition(pos);
        }

        public override void Render()
        {
            if(FlyTimer.IsRunning == false) return;
            
            float remainingTime = FlyTimer.RemainingTime(Runner).Value;

            float t = 1 - (remainingTime / FlyTargetTime);
            
            float sizeValue = GameSettingProvider.GameSettings.GrenadeFlySizeFunction.Evaluate(t);

            View.localScale = Vector3.Lerp(OriginSize, TargetSize, sizeValue);
        }
    }
}