using System;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Dev.Weapons
{
    public class Turret : NetworkContext
    {
        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private float _detectionRadius = 15;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private TurretView _turretView;

        [SerializeField] private float _haoticDirectionSpeed = 0.5f;
        
        private PlayerRef _owner;
        private TeamsService _teamsService;
        private TeamSide _ownerTeamSide;

        private PlayerCharacter _targetPlayer;

        [Networked] private PlayerRef TargetPlayerId { get; set; }

        private TickTimer _directionChooseTimer;
        private Vector2 _direction;

        public Subject<Unit> ToDie { get; } = new Subject<Unit>();

        [Inject]
        private void Construct(TeamsService teamsService)
        {
            _teamsService = teamsService;
        }
        
        public void Init(PlayerRef owner)
        {
            _owner = owner;
            _ownerTeamSide = _teamsService.GetUnitTeamSide(owner);
            
            Observable.Interval(TimeSpan.FromSeconds(0.5f)).TakeUntilDestroy(this).Subscribe((l =>
            {
                if(HasStateAuthority == false) return;
                
                SearchForTargets();
            }));
            
        }
        
        private void SearchForTargets()
        {
            bool overlapSphere = Extensions.OverlapSphere(Runner, transform.position, _detectionRadius, _playerLayer, out var colliders);

            bool playerFound = false;
            
            if (overlapSphere)
            {
                foreach (Collider2D collider in colliders)
                {
                    bool isPlayer = collider.TryGetComponent<PlayerCharacter>(out var playerCharacter);

                    if (isPlayer)
                    {
                        TeamSide playerTeamSide = _teamsService.GetUnitTeamSide(playerCharacter.Object.InputAuthority);

                        if (playerTeamSide == _ownerTeamSide)
                        {
                            continue;
                        }
                        else
                        {
                            playerFound = true;
                            
                            if (TargetPlayerId != null)
                            {
                                if (playerCharacter.Object.InputAuthority != TargetPlayerId)
                                {
                                    Debug.Log($"Found new target! {playerCharacter.Object.InputAuthority.PlayerId}", playerCharacter);
                                    TargetPlayerId = playerCharacter.Object.InputAuthority;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                Debug.Log($"Found new target! {playerCharacter.Object.InputAuthority.PlayerId}", playerCharacter);
                                TargetPlayerId = playerCharacter.Object.InputAuthority;
                            }
                        }
                        
                    }
                }
            }

            if (playerFound == false)
            {
                TargetPlayerId = PlayerRef.None;
            }
            
        }
        
        public override void FixedUpdateNetwork()
        {
            if(HasStateAuthority == false) return;
            
            if(TargetPlayerId == PlayerRef.None)
            {
                HaoticDirectionMovement();
                
                return;
            }
            
            if (_targetPlayer == null)
            {
                _targetPlayer = Runner.GetPlayerObject(TargetPlayerId).GetComponent<PlayerCharacter>();
            }
            
            Vector2 direction = (_targetPlayer.transform.position - transform.position).normalized;

            _direction = direction;
            _turretView.transform.up = direction;
            
            _weaponController.AimWeaponTowards(direction);
            _weaponController.TryToFire(direction);
        }

        private void HaoticDirectionMovement()
        {
            if (_directionChooseTimer.ExpiredOrNotRunning(Runner))
            {
                _directionChooseTimer = TickTimer.CreateFromSeconds(Runner, 1);
                _direction = Random.onUnitSphere;
            }

            Vector2 lerperDirection = Vector2.Lerp(_turretView.transform.up, _direction, Runner.DeltaTime * _haoticDirectionSpeed);
            
            _turretView.transform.up = lerperDirection;
            _weaponController.AimWeaponTowards(lerperDirection);
        }
        
        
        
    }
}