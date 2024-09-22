using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons;
using Dev.Weapons.Guns;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Random = UnityEngine.Random;

namespace Dev.BotsLogic
{
    [SelectionBase]
    public class Bot : NetworkContext, IDamageable
    {
        public DamagableType DamageId => DamagableType.Bot;

        [SerializeField] private Collider2D _collider;
        
        [SerializeField] private NavMeshAgent _navMeshAgent;

        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private bool _allowToShoot = true;
        [SerializeField] private bool _allowToMove = true;

        [SerializeField] private Rigidbody2D _rigidbody;

        [SerializeField] private float _speed = 1.2f;
        [SerializeField] private float _chaseSpeed = 3.5f;

        [SerializeField] private LayerMask _playerLayer;

        [SerializeField] private float _moveDistance = 10;
        [SerializeField] private BotView _view;

        private Vector3 _movePointPos;
        private int _currentPointIndex = 0;
        private List<BotMovePoint> MovePoints => _botsController.LevelMovePoints;

        private TeamsService _teamsService;
        private GameSettings _gameSettings;
        private BotsController _botsController;

        public bool Alive = true;

        [Networked] private NetworkObject Target { get; set; }
        [Networked] private NetworkBool IsFrozen { get; set; }
        [Networked] public BotData BotData { get; private set; }

        public BotView View => _view;
        public TeamSide BotTeamSide => BotData.TeamSide;
        public NavMeshAgent NavMeshAgent => _navMeshAgent;

        protected override void Awake()
        {
            base.Awake();
            _navMeshAgent.updateUpAxis = false;
            _navMeshAgent.updateRotation = false;
        }

        public void Init(BotData botData)
        {
            BotData = botData;

            _weaponController.RPC_SetOwner(BotData.SessionPlayer);
        }

        [Inject]
        private void Construct(TeamsService teamsService, GameSettings gameSettings, BotsController botsController)
        {
            _botsController = botsController;
            _gameSettings = gameSettings;
            _teamsService = teamsService;
        }

        [Rpc]
        public void RPC_OnDeath(bool isDead) // TODO bullshit
        {
            if (isDead)
            {
                transform.DOScale(0, 0.5f);
            }
            else
            {
                transform.DOScale(1, 0.5f);
            }
            
            Alive = !isDead;
            _collider.enabled = !isDead;
        }
    
        public override void Spawned()
        {
            base.Spawned();

            ChangeMoveDirection();

            Observable.Interval(TimeSpan.FromSeconds(_gameSettings.BotsSearchForTargetsCooldown)).TakeUntilDestroy(this)
                .Subscribe((l =>
                {
                    if (HasStateAuthority == false) return;

                    SearchForTargets();
                }));

            Observable.Interval(TimeSpan.FromSeconds(_gameSettings.BotsChangeMoveDirectionCooldown))
                .TakeUntilDestroy(this).Subscribe((l =>
                {
                    if (HasStateAuthority == false) return;

                    ChangeMoveDirection();
                }));
        }

        public void SetFreezeState(bool toFreeze)
        {
            IsFrozen = toFreeze;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            if (Alive == false) return;

            if (IsFrozen) return;

            if(_allowToMove == false) return;
            
            if (Target != null)
            {
                MoveToTarget();
            }
            else
            {
                Move(_movePointPos, _speed);
            }
        }

        public override void Render()
        {
            _view.RPC_OnMove(_navMeshAgent.velocity.magnitude / _navMeshAgent.speed, Mathf.Sign(_navMeshAgent.velocity.x) < 0);
        }

        private void SearchForTargets()
        {
            bool overlapSphere = Extensions.OverlapCircle(Runner, transform.position, _gameSettings.BotsTargetsSearchRadius, out var colliders);

            bool targetFound = false;

            if (overlapSphere)
            {
                foreach (Collider2D collider in colliders)
                {
                    bool isDamagable = collider.TryGetComponent<IDamageable>(out var damagable);

                    if (isDamagable == false) continue;

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

                        if (playerTeamSide != BotTeamSide)
                        {
                            targetFound = true;

                            if (TryAssignTarget(playerCharacter.Object, playerTeamSide))
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
            if (targetTeam == BotTeamSide) return false;

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

        private void MoveToTarget()
        {
            Vector3 direction = (Target.transform.position - transform.position).normalized;
            Vector3 movePos = transform.position + direction;

            Move(movePos, _chaseSpeed);

            _weaponController.AimWeaponTowards(direction);

            if (_allowToShoot)
            {
                _weaponController.TryToFire(direction);
            }
        }

        private void ChangeMoveDirection()
        {
            if (HasStateAuthority == false) return;

            var movePoints = MovePoints.OrderBy(x => (x.transform.position - transform.position).sqrMagnitude)
                .ToList();

            int maxPoints = _gameSettings.BotsNearestPointsAmountToChoose;
            int index = Math.Clamp(Random.Range(0, maxPoints), 0, movePoints.Count());

            BotMovePoint movePoint = movePoints[index];

            _movePointPos = movePoint.transform.position;
        }

        private void Move(Vector3 movePos, float speed)
        {
            //_rigidbody.position = Vector3.MoveTowards(transform.position, movePos, Runner.DeltaTime * speed);
            _navMeshAgent.speed = speed;
            _navMeshAgent.SetDestination(movePos);
        }

        public static implicit operator int(Bot bot)
        {
            return (int)bot.Object.Id.Raw;
        }
    }
}