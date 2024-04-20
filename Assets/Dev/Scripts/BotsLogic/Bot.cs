using System;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons;
using Dev.Weapons.Guns;
using Fusion;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.BotsLogic
{
    public class Bot : NetworkContext, IDamageable
    {
        public int DamageId => AtomicConstants.DamageIds.BotDamageId;
        
        [SerializeField] private WeaponController _weaponController;

        [SerializeField] private Rigidbody2D _rigidbody;
            
        [SerializeField] private float _speed = 1.2f;

        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _searchRadius = 5;

        [SerializeField] private float _moveDistance = 10;
        [SerializeField] private BotView _view;

        public BotView View => _view;

        private PlayerCharacter _targetPlayer;
        private BotData _botData;   
        private Vector3 _movePos;
        private int _currentPointIndex = 0;
        
        private TeamsService _teamsService;

        public BotData BotData => _botData;
        [Networked] private PlayerRef TargetPlayerId { get; set; }

        public bool Alive = true;
        
        public void Init(BotData botData)
        {
            _botData = botData;
        }

        private void Start()
        {
            _teamsService = DependenciesContainer.Instance.GetDependency<TeamsService>();
        }

        public override void Spawned()
        {
            Debug.Log($"Bot spawn");

            base.Spawned();
            
            ChangeMoveDirection();
            
            Observable.Interval(TimeSpan.FromSeconds(0.5f)).Subscribe((l =>
            {
                if(HasStateAuthority == false) return;
                
                SearchForTargets();
            }));
            
            Observable.Interval(TimeSpan.FromSeconds(3)).Subscribe((l =>
            {
                if(HasStateAuthority == false) return;
                
                ChangeMoveDirection();
            }));
            
        }

        public override void FixedUpdateNetwork()
        {
            Debug.Log($"Bot update");
            if(HasStateAuthority == false) return;
            
            if(Alive == false) return;
            
            if (TargetPlayerId != PlayerRef.None)
            {
                MoveToTarget();
            }
            else
            {
                Move(_movePos);
            }
        }

        private void SearchForTargets()
        {
            bool overlapSphere = Extensions.OverlapSphere(Runner, transform.position, _searchRadius, _playerLayer, out var colliders);

            bool playerFound = false;
            
            if (overlapSphere)
            {
                foreach (Collider2D collider in colliders)
                {
                    bool isPlayer = collider.TryGetComponent<PlayerCharacter>(out var playerCharacter);

                    if (isPlayer)
                    {
                        TeamSide playerTeamSide = _teamsService.GetUnitTeamSide(playerCharacter.Object.InputAuthority);

                        if (playerTeamSide == _botData.TeamSide)
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

        private void MoveToTarget()
        {
            if (_targetPlayer == null)
            {
                _targetPlayer = Runner.GetPlayerObject(TargetPlayerId).GetComponent<PlayerCharacter>();
            }
            
            Vector2 direction = (_targetPlayer.transform.position - transform.position).normalized;
            Vector2 movePos = direction * _speed;
            
            Move(movePos);
            
            _weaponController.AimWeaponTowards(direction);
            //_weaponController.TryToFire(direction);
        }

        private void ChangeMoveDirection()
        {
            _movePos = transform.position + Random.onUnitSphere * Random.Range(1f, _moveDistance);
        }
        
        private void Move(Vector3 movePos)
        {
            _rigidbody.position = Vector3.MoveTowards(transform.position, movePos, Runner.DeltaTime * _speed);
        }


        public static implicit operator int(Bot bot)
        {
            return (int)bot.Object.Id.Raw;
        }
        
    }
}