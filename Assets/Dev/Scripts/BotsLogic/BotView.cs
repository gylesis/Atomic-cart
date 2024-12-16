using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using DG.Tweening;
using Fusion;
using UnityEngine;

namespace Dev.BotsLogic
{
    public class BotView : NetworkContext
    {
        [SerializeField] private SpriteRenderer _teamBanner;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _sprite;

        private static readonly int Move = Animator.StringToHash("Move");

        [Networked] private Color TeamBannerColor { get; set; }

        protected override void CorrectState()
        {
            base.CorrectState();
            
            UpdateTeamBannerColor();
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_SetTeamBannerColor(Color color)
        {
            TeamBannerColor = color;
            UpdateTeamBannerColor();
        }

        private void UpdateTeamBannerColor()
        {
            _teamBanner.color = TeamBannerColor;
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_Scale(float target)
        {
            transform.DOScale(target, 0.5f);
        }
        
        
        [Rpc]
        public void RPC_OnMove(float velocity, bool isRight)
        {
            _animator.SetFloat(Move, velocity);

            SetFlipSide(isRight);
        }
        
        private void SetFlipSide(bool isRight)
        {
            _sprite.flipX = isRight;
        }
    }
}