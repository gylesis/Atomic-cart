using Dev.Infrastructure;
using Dev.Utils;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public class GrenadeProjectile : ExplosiveProjectile
    {
        private Vector2 _originPos;
        private Vector2 _targetPos;
        private float _flyTargetTime;

        private TickTimer _flyTimer;

        private bool _hasFlied;
        private Vector3 _targetSize;
        private Vector3 _originSize;

        private TickTimer DetonateTimer;
            
        public void Init(Vector3 moveDirection, float force, int damage, PlayerRef owner, float explosionRadius, Vector2 targetPos, float flyTime)
        {
            _flyTargetTime = flyTime;
            _originPos = transform.position;
            _targetPos = targetPos;
            
            _flyTimer = TickTimer.CreateFromSeconds(Runner, _flyTargetTime);
            _targetSize = View.localScale * 2;
            _originSize = View.localScale;
            
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
            
            if(_flyTimer.IsRunning == false) return;
            
            if(_hasFlied) return;

            if (_flyTimer.Expired(Runner))
            {
                _hasFlied = true;
                DetonateTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);
                return;
            }

            MoveAlongDirection();
        }

        private void MoveAlongDirection()
        {
            if(_flyTimer.IsRunning == false) return;
            
            float remainingTime = _flyTimer.RemainingTime(Runner).Value;

            float t = 1 - (remainingTime / _flyTargetTime);
            float value = GameSettingProvider.GameSettings.GrenadeFlyFunction.Evaluate(t);

            float sizeValue = GameSettingProvider.GameSettings.GrenadeFlySizeFunction.Evaluate(t);

            View.localScale = Vector3.Lerp(_originSize, _targetSize, sizeValue);

            Vector2 pos = Vector2.Lerp(_originPos, _targetPos, value);

            _networkRigidbody2D.Rigidbody.MovePosition(pos);
        }
    }
}