﻿using System;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons;
using Fusion;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.BotsLogic
{
    public class Bot : NetworkContext
    {
        [SerializeField] private WeaponController _weaponController;

        [SerializeField] private Rigidbody2D _rigidbody;
            
        [SerializeField] private float _speed = 1.2f;

        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _searchRadius = 5;

        [SerializeField] private float _moveDistance = 10;
        
        private int _currentPointIndex = 0;
        private BotData _botData;   
        
        private TeamsService _teamsService;
        [Networked] private PlayerRef TargetPlayerId { get; set; }
        
        private PlayerCharacter _targetPlayer;
        
        private Vector3 _movePos;
        
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
                        TeamSide playerTeamSide = _teamsService.GetPlayerTeamSide(playerCharacter.Object.InputAuthority);

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
            _weaponController.TryToFire(direction);
        }

        private void ChangeMoveDirection()
        {
            _movePos = transform.position + Random.onUnitSphere * Random.Range(1f, _moveDistance);
        }
        
        private void Move(Vector3 movePos)
        {
            _rigidbody.position = Vector3.MoveTowards(transform.position, movePos, Runner.DeltaTime * _speed);
        }
    }
}