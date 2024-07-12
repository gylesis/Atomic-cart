using Dev.Infrastructure;
using DG.Tweening;
using Fusion;
using UnityEngine;

namespace Dev.BotsLogic
{
    public class BotView : NetworkContext
    {
        [SerializeField] private SpriteRenderer _teamBanner;

        [Networked] private Color TeamBannerColor { get; set; }

        protected override void CorrectState()
        {
            base.CorrectState();
            
            UpdateTeamBannerColor();
        }

        [Rpc]
        public void RPC_SetTeamBannerColor(Color color)
        {
            TeamBannerColor = color;
        }

        private void UpdateTeamBannerColor()
        {
            _teamBanner.color = TeamBannerColor;
        }

        [Rpc]
        public void RPC_Scale(float target)
        {
            transform.DOScale(target, 0.5f);
        }
    }
}