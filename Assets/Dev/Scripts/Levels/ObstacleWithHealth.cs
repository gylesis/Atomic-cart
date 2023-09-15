using DG.Tweening;
using Fusion;
using UnityEngine;

namespace Dev.Levels
{
    public class ObstacleWithHealth : Obstacle
    {
        [SerializeField] private HitboxRoot _hitboxRoot;
        [SerializeField] protected int _health = 50;
        public int Health => _health;
        public override int Id => 0;

        protected override void CorrectState()
        {
            base.CorrectState();

            _hitboxRoot.HitboxRootActive = IsActive;
        }

        public virtual void OnZeroHealth()
        {
            RPC_SetHitboxState(false);

            RPC_DoScale(0.5f, 0);

            DOVirtual.DelayedCall(0.5f, (() =>
            {
                IsActive = false;
            }));
        }

        public virtual void Restore()
        {
            RPC_DoScale(0.5f, 1, Ease.OutBounce);
            
            DOVirtual.DelayedCall(0.5f, (() =>
            {
                IsActive = true;
                RPC_SetHitboxState(true);
            }));
        }

        [Rpc]
        private void RPC_SetHitboxState(bool isOn)
        {
            _hitboxRoot.HitboxRootActive = isOn;
        }
        
    }
}