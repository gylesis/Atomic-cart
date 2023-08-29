using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class PlayerView : NetworkContext
    {
        [SerializeField] private NetworkMecanimAnimator _networkAnimator;
        [SerializeField] private SpriteRenderer _teamBanner;

        [SerializeField] private SpriteRenderer _playerSprite;

        private static readonly int Move = Animator.StringToHash("Move");

        [Networked(OnChanged = nameof(OnTeamColorChanged))] private Color TeamColor { get; set; }

        public void OnMove(float velocity, bool isRight)
        {
            _networkAnimator.Animator.SetFloat(Move, velocity);

            RPC_SetFlipSide(isRight);
        }

        [Rpc]
        private void RPC_SetFlipSide(bool isRight)
        {
            _playerSprite.flipX = isRight;
        }

        private static void OnTeamColorChanged(Changed<PlayerView> changed)
        {
            changed.Behaviour._teamBanner.color = changed.Behaviour.TeamColor;
        }
        
        [Rpc]
        public void RPC_SetTeamColor(Color color)
        {
            TeamColor = color;
        }
    }
}