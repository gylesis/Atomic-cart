using System;
using Dev.Infrastructure;
using Dev.UI;
using Dev.Weapons;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class PlayerController : NetworkContext
    {
        [SerializeField] private Player _player;
        private PopUpService _popUpService;
        private WeaponController _weaponController => _player.WeaponController;
        private float Speed => _player.Speed;
        private PlayerView PlayerView => _player.PlayerView;
        private float ShootThreshold => _player.ShootThreshold;
        
        [Networked] private Vector2 LastMoveDirection { get; set; }
        [Networked] private Vector2 LastLookDirection { get; set; }

        public bool AllowToMove { get; set; } = true;

        public bool AllowToShoot { get; set; } = true;

        private void Awake()
        {
            _popUpService = FindObjectOfType<PopUpService>();
        }

        public override void FixedUpdateNetwork()
        {
            var hasInput = GetInput<PlayerInput>(out var input);

            if (hasInput == false) return;

            if (AllowToMove)
            {
                _player.Rigidbody.velocity = input.MoveDirection * Speed * Runner.DeltaTime;
            }

            HandleAnimation(input);

            if (AllowToShoot)
            {
                AimRotation(input);
            }
        }

        public override void Render()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                var tryGetPopUp = _popUpService.TryGetPopUp<PlayersScoreMenu>(out var scoreMenu);
                scoreMenu.Show();
            }
            
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                var tryGetPopUp = _popUpService.TryGetPopUp<PlayersScoreMenu>(out var scoreMenu);
                scoreMenu.Hide();
            }
            
        }

        private void HandleAnimation(PlayerInput input)
        {
            Vector2 moveDirection = input.MoveDirection;
            
            float sign = 1;

            if (moveDirection == Vector2.zero)
            {
                sign = Mathf.Sign(Vector2.Dot(Vector2.left, LastMoveDirection));
            }
            else
            {
                LastMoveDirection = moveDirection;
                sign = Mathf.Sign(Vector2.Dot(Vector2.left, moveDirection));
            }

            bool isRight = sign == 1;

            PlayerView.OnMove(moveDirection.magnitude, isRight);
        }

        private void AimRotation(PlayerInput input)
        {
            if (_weaponController.HasAnyWeapon == false) return;

            Vector2 lookDirection = input.LookDirection;

            if (input.LookDirection == Vector2.zero)
            {
                lookDirection = LastLookDirection;
            }
            else
            {
                LastLookDirection = lookDirection;

                var magnitude = lookDirection.sqrMagnitude;

                if (magnitude >= ShootThreshold)
                {
                    Shoot();
                }
            }

            _weaponController.AimWeaponTowards(lookDirection);
        }

        private void Shoot()
        {
            _weaponController.TryToFire();
        }
        
    }
}