using System;
using Dev.BotsLogic;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons.Guns;
using Fusion;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Dev.Weapons
{
    public class Landmine : ExplosiveProjectile
    {
        [SerializeField] private float _detonateSeconds;
        [SerializeField] private float _searchRadius = 5;
        [SerializeField] private float _detonateRadius = 5;
        
        private TeamSide _ownerTeam;

        private bool _hasAnyTarget;

        private TickTimer _detonateTimer;

        private bool _exploding;
        
        public void Init(TeamSide teamSide)
        {
            _ownerTeam = teamSide;
        }

        public override void FixedUpdateNetwork()
        {
            if(HasStateAuthority == false) return;

            if(_exploding) return;
            
            if (_detonateTimer.Expired(Runner))
            {
                Explode();
            }
            
            if(_hasAnyTarget) return;
            
            bool overlapSphere = Extensions.OverlapCircle(Runner, transform.position, _searchRadius, LayerMask.GetMask("Player", "Bot"), out var colliders);

            if (overlapSphere)
            {
                foreach (Collider2D collider in colliders)
                {
                    bool isBot = collider.TryGetComponent<Bot>(out var bot);
                    bool isPlayer = collider.TryGetComponent<PlayerCharacter>(out var playerCharacter);

                    if (isBot)
                    {
                        TeamSide targetTeam = bot.BotData.TeamSide;

                        if (targetTeam != _ownerTeam)
                        {
                            StartDetonation();
                            _hasAnyTarget = true;
                            break;
                        }

                    }

                    if (isPlayer)
                    {
                        TeamSide targetTeam = playerCharacter.TeamSide;

                        if (targetTeam != _ownerTeam)
                        {
                            StartDetonation();
                            _hasAnyTarget = true;
                            break;
                        }
                    }
                    
                }
            }
            
        }

        private void Explode()
        {
            Debug.Log($"Explode");
            ExplodeAndHitPlayers(_detonateRadius);
            _exploding = true;

            Observable.Timer(TimeSpan.FromSeconds(3)).TakeUntilDestroy(this).Subscribe((l =>
            {
                ToDestroy.OnNext(this);
            }));
        }

        private void StartDetonation()
        {
            Debug.Log($"Start detonation");
            _detonateTimer = TickTimer.CreateFromSeconds(Runner, _detonateSeconds);
        }

        private void OnDrawGizmosSelected()
        {
            var position = transform.position;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(position, _searchRadius);
            Handles.Label(position + Vector3.up * _searchRadius, "Search radius");
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, _detonateRadius);
            Handles.Label(position + Vector3.up * _detonateRadius + Vector3.right, "Detonate radius");
        }
    }
}