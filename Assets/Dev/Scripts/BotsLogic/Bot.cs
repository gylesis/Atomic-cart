﻿using System;
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
using Object = UnityEngine.Object;
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

        private int _currentPointIndex = 0;
        public List<BotMovePoint> MovePoints => _botsController.LevelMovePoints;

        private BotsController _botsController;
        private BotStateController _botStateController;

        public bool Alive = true;
        private GameSettings _gameSettings;
        private SessionStateService _sessionStateService;

        [Networked] public NetworkObject Target { get; set; }
        [Networked] public NetworkBool IsFrozen { get; set; }
        [Networked] public BotData BotData { get; private set; }


        public SessionPlayer TargetSessionPlayer => Target != null ? _sessionStateService.GetSessionPlayer(Target.Id) : default(SessionPlayer);
        
        public Vector3 RandomMovePointPos { get; private set; }
        public WeaponController WeaponController => _weaponController;
        public bool AllowToShoot => _allowToShoot;
        public Rigidbody2D Rigidbody => _rigidbody;
        public float Speed => _speed;
        public float ChaseSpeed => _chaseSpeed;

        public bool AllowToMove => _allowToMove;
        public BotView View => _view;
        public TeamSide BotTeamSide => BotData.TeamSide;
        public NavMeshAgent NavMeshAgent => _navMeshAgent;

        public BotStateController BotStateController => _botStateController;

        public Vector2 DirectionToTarget => (Target.transform.position - transform.position).normalized;
        public Vector2 DirectionToMovePos => (RandomMovePointPos - transform.position).normalized;
        
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
        private void Construct(BotsController botsController, BotStateController botStateController, GameSettings gameSettings, SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
            _gameSettings = gameSettings;
            _botStateController = botStateController;
            _botsController = botsController;
        }

        [Rpc]
        public void RPC_OnDeath(bool isDead) // TODO bullshit
        {
            transform.DOScale(isDead ? 0 : 1, 0.5f);

            Alive = !isDead;
            _collider.enabled = !isDead;
        }
    
        public void SetFreezeState(bool toFreeze)
        {
            IsFrozen = toFreeze;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            _botStateController.FixedNetworkTick();
        }
        
        public void ChangeMoveDirection()
        {
            if (HasStateAuthority == false) return;

            var movePoints = MovePoints.OrderBy(x => (x.transform.position - transform.position).sqrMagnitude)
                .ToList();

            int maxPoints = _gameSettings.BotsConfig.BotsNearestPointsAmountToChoose;
            int index = Math.Clamp(Random.Range(0, maxPoints), 0, movePoints.Count());

            BotMovePoint movePoint = movePoints[index];

            RandomMovePointPos = movePoint.transform.position;
        }
        
        public bool TryAssignTarget(NetworkObject potentialTarget, TeamSide targetTeam)
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
        
        public bool TryFindNearTarget()
        {
            bool overlapSphere = Extensions.OverlapCircle(_botStateController.NetworkRunner, transform.position, _gameSettings.BotsConfig.BotsTargetsSearchRadius, out var colliders);

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

                        TeamSide botSide = _botStateController.TeamsService.GetUnitTeamSide(bot);

                        if (TryAssignTarget(bot.Object, botSide))
                        {
                            targetFound = true;
                            break;
                        }
                    }

                    if (isPlayer)
                    {
                        PlayerCharacter playerCharacter = damagable as PlayerCharacter;

                        TeamSide playerTeamSide = _botStateController.TeamsService.GetUnitTeamSide(playerCharacter.Object.InputAuthority);

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
            
            return targetFound;
        }
        
        public void MoveTowardsTarget()
        {
            Vector3 direction = (Target.transform.position - transform.position).normalized;
            Vector3 movePos = transform.position + direction;

            Move(movePos, _chaseSpeed);
        }

        public void AimWeaponTowards(Vector2 direction)
        {
            _weaponController.AimWeaponTowards(direction);
        }

        public void Move(Vector3 movePos)
        {
            Move(movePos, Speed);
        }
        
        public void Move(Vector3 movePos, float speed)
        {
            _navMeshAgent.speed = speed;
            _navMeshAgent.SetDestination(movePos);
        }

        public override void Render()
        {
            if(HasStateAuthority == false) return;
            
            _view.RPC_OnMove(_navMeshAgent.velocity.magnitude / _navMeshAgent.speed, Mathf.Sign(_navMeshAgent.velocity.x) < 0);
        }

        public static implicit operator int(Bot bot)
        {
            return (int)bot.Object.Id.Raw;
        }

        private void OnDestroy()
        {
            _botStateController.StateMachine.Exit();
        }
        
    }
}