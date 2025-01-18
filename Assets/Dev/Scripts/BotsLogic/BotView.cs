using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;

namespace Dev.BotsLogic
{
    public class BotView : NetworkContext
    {
        [SerializeField] private SpriteRenderer _teamBanner;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _botSprite;

        private static readonly int Move = Animator.StringToHash("Move");

        [Networked] private Color TeamBannerColor { get; set; }

        protected override void CorrectState()
        {
            base.CorrectState();
            
            UpdateTeamBannerColor();
        }

        private void UpdateTeamBannerColor()
        {
            _teamBanner.color = TeamBannerColor;
        }

        public void UpdateCharacterView(AnimatorOverrideController animatorController, Sprite sprite)
        {
            _animator.runtimeAnimatorController = null;
            _botSprite.sprite = sprite;
            _animator.runtimeAnimatorController = animatorController;
        }


        [Rpc]
        public void RPC_OnMove(float velocity, bool isRight)
        {
            _animator.SetFloat(Move, velocity);
            SetFlipSide(isRight);
        }

        private void SetFlipSide(bool isRight)
        {
            _botSprite.flipX = isRight;
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_SetTeamBannerColor(Color color)
        {
            TeamBannerColor = color;
            UpdateTeamBannerColor();
        }
    }
}