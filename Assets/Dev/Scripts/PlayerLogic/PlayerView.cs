using Dev.Infrastructure;
using Dev.Utils;
using Dev.Weapons.Guns;
using Fusion;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerView : NetworkContext
    {
        [SerializeField] private NetworkMecanimAnimator _networkAnimator;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _teamBanner;

        [SerializeField] private SpriteRenderer _playerSprite;

        [SerializeField] private Transform _groundAimTransform;
        [SerializeField] private float _aimLerpSpeed = 1;

        [SerializeField] private SpriteRenderer _crosshairSpriteRenderer;

        private static readonly int Move = Animator.StringToHash("Move");
        private PlayerBase _playerBase;

        public Vector3 CrosshairPos => _groundAimTransform.TransformVector(_groundAimTransform.position);
        
        
        [Networked]
        private Color TeamColor { get; set; }

        protected override void CorrectState()
        {
            base.CorrectState();
            
            OnTeamColorChanged();
        }

        public void OnMove(float velocity, bool isRight)
        {
            _animator.SetFloat(Move, velocity);

            SetFlipSide(isRight);
        }

        private void SetFlipSide(bool isRight)
        {
            _playerSprite.flipX = isRight;
        }

        private void OnTeamColorChanged()
        {
            _teamBanner.color = TeamColor;
        }

        [Rpc]
        public void RPC_SetTeamColor(Color color)
        {
            TeamColor = color;
            OnTeamColorChanged();
        }

        public override void Render()
        {
            if(HasStateAuthority == false) return;
            
            if (_playerBase == null && PlayerCharacter.LocalPlayerCharacter != null)
            {
                _playerBase = PlayerBase.LocalPlayerBase;
            }
            
            if(_playerBase.Character == null) return;

            float crosshairColorTarget = _playerBase.PlayerController.IsPlayerAiming ? 1 : 0;

            Color color = _crosshairSpriteRenderer.color;
            color.a = Mathf.Lerp(color.a, crosshairColorTarget, Runner.DeltaTime * 20);
            _crosshairSpriteRenderer.color = color;

            Vector3 lookDirection;

            if (_playerBase.PlayerController.IsCastingMode)
            {
                lookDirection = _playerBase.PlayerController.LastLookDirection;
            }
            else
            {
                lookDirection = _playerBase.PlayerController.LastLookDirection.normalized;
            }
            
            Weapon weapon = _playerBase.Character.WeaponController.CurrentWeapon;
            float aimDistance = Extensions.AtomicCart.GetBulletMaxDistanceClampedByWalls(_playerBase.transform.position, weapon.ShootDirection, 
                weapon.BulletMaxDistance, weapon.BulletHitOverlapRadius + 0.05f);

            Vector3 targetPos = Vector3.zero + lookDirection * aimDistance;
            
            _groundAimTransform.localPosition = Vector3.Lerp(_groundAimTransform.localPosition,
                targetPos, _aimLerpSpeed * Runner.DeltaTime);
        }
    }
}