using Dev.Utils;
using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Levels
{
    public class ObstacleWithHealth : Obstacle
    {
        [SerializeField] private Collider2D _collider;
        [SerializeField] protected int _health = 50;
        public int Health => _health;
        public override int DamageId => AtomicConstants.DamageIds.ObstacleWithHealthDamageId;

        protected override void CorrectState()
        {
            base.CorrectState();

            _collider.enabled = IsActive;
        }

        public virtual void OnZeroHealth()
        {
            RPC_SetHitboxState(false);

            RPC_DoScale(0.5f, 0);

            DOVirtual.DelayedCall(0.5f, (() =>
            {
               RPC_SetActive(false);
            }));
        }

        public virtual void Restore()
        {
            RPC_DoScale(0.5f, 1, Ease.OutBounce);
            
            DOVirtual.DelayedCall(0.5f, (() =>
            {
                RPC_SetActive(true);
                RPC_SetHitboxState(true);
            }));
        }

        [Rpc]
        private void RPC_SetHitboxState(bool isOn)
        {
            _collider.enabled = isOn;
        }
        
    }
}