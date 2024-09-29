using System;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons.Guns;
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
        
        private SessionPlayer _owner;
        private TeamSide _ownerTeamSide;
        private TickTimer _directionChooseTimer;
        private Vector2 _direction;

        private TeamsService _teamsService;
        
        [Networked] private NetworkObject Target { get; set; }

        public Subject<Unit> ToDie { get; } = new Subject<Unit>();
        private bool HasTarget => Target != null;
        
        [Inject]
        private void Construct(TeamsService teamsService)
        {
            _teamsService = teamsService;
        }
        
        public void Init(SessionPlayer owner)
        {
            _owner = owner;
            _ownerTeamSide = _owner.TeamSide;

            _weaponController.RPC_SetOwner(_owner);
            
            Observable.Interval(TimeSpan.FromSeconds(0.5f)).TakeUntilDestroy(this).Subscribe((l =>
            {
                if(HasStateAuthority == false) return;
                
                SearchForTargets();
            }));
            
        }
        
        private void SearchForTargets()
        {
            bool overlapSphere = Extensions.OverlapCircleWithWalls(Runner, transform.position, _detectionRadius, out var targets);

            bool targetFound = false;
            
            if (overlapSphere)
            {
                foreach (Collider2D target in targets)
                {
                    bool isDamagable = target.TryGetComponent<IDamageable>(out var damagable);
                    
                    if(isDamagable == false) continue;
                    
                    bool isDummyTarget = damagable.DamageId == DamagableType.DummyTarget;
                    bool isBot = damagable.DamageId == DamagableType.Bot;
                    bool isStaticObstacle = damagable.DamageId == DamagableType.Obstacle;
                    bool isObstacleWithHealth = damagable.DamageId == DamagableType.ObstacleWithHealth;
                    bool isPlayer = damagable.DamageId == DamagableType.Player;

                    if (isBot)
                    {
                        Bot bot = damagable as Bot;

                        TeamSide botSide = _teamsService.GetUnitTeamSide(bot);

                        if (TryAssignTarget(bot.Object, botSide))
                        {
                            targetFound = true;
                            break;
                        }
                        
                    }
                    
                    if (isPlayer)
                    {
                        PlayerCharacter playerCharacter = damagable as PlayerCharacter;
                        
                        TeamSide playerTeamSide = _teamsService.GetUnitTeamSide(playerCharacter.Object.InputAuthority);

                        if (TryAssignTarget(playerCharacter.Object, playerTeamSide))
                        {
                            targetFound = true;
                            break;
                        }
                        
                    }
                    
                }
            }

            if (targetFound == false)
            {
                Target = null;
            }
            
        }

        private bool TryAssignTarget(NetworkObject potentialTarget, TeamSide targetTeam)
        {
            if (targetTeam == _ownerTeamSide) return false;
            
            if (Target != null)
            {
                bool isSameTarget = potentialTarget.Id == Target.Id;
                
                if (isSameTarget)
                {
                    return false;
                }
                else
                {
                    //Debug.Log($"Found new target!", potentialTarget.gameObject);
                    Target = potentialTarget;
                    return true;
                }
            }
            else
            {
                //Debug.Log($"Found target!", potentialTarget.gameObject);
                Target = potentialTarget;
                return true;
            }
        }
        
        public override void FixedUpdateNetwork()
        {
            if(HasStateAuthority == false) return;
            
            if(HasTarget == false)
            {
                HaoticDirectionMovement();
                
                return;
            }
            
            Vector2 direction = (Target.transform.position - transform.position).normalized;

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