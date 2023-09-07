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

        [SerializeField] private Transform _groundAimTransform;
        [SerializeField] private float _aimDistance = 5;
        [SerializeField] private float _aimLerpSpeed = 1;

        [SerializeField] private SpriteRenderer _crosshairSpriteRenderer;
        
        private static readonly int Move = Animator.StringToHash("Move");
        private Player _player;

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

        public override void Render()
        {
            if (_player == null)
            {
                _player = Runner.GetPlayerObject(Object.InputAuthority).GetComponent<Player>();
            }
            
            float crosshairColorTarget = _player.PlayerController.IsPlayerAiming ? 1 : 0;

            Color color = _crosshairSpriteRenderer.color;

            color.a = Mathf.Lerp(color.a, crosshairColorTarget, Runner.DeltaTime * 20);

            _crosshairSpriteRenderer.color = color;
            
            Vector3 lookDirection = _player.PlayerController.LastLookDirection;
            
            _groundAimTransform.localPosition = Vector3.Lerp(_groundAimTransform.localPosition,
                Vector3.zero + lookDirection * _aimDistance, _aimLerpSpeed * Runner.DeltaTime);
        }
    }
}