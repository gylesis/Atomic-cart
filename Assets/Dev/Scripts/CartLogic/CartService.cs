using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.CartLogic
{
    public class CartService : NetworkContext
    {
        [Header("Cart Settings")] [SerializeField]
        private float _cartVelocity = 2f;

        [SerializeField] private float _cartRotationSpeed = 1f;

        [Space] [SerializeField] private Cart _cart;
        [SerializeField] private List<CartPathPoint> _pathPoints;

        [SerializeField] private TeamSide _teamToCapturePoints = TeamSide.Red;

        public TeamSide TeamToCapturePoints => _teamToCapturePoints;

        private CartPathPoint _currentPoint;
        private CartPathPoint _nextPoint;
        private CartPathPoint _prevPoint;

        private int _currentPointIndex = 0;

        [Networked] private NetworkBool AllowToMove { get; set; }

        public bool IsOnLastPoint => _currentPointIndex >= _pathPoints.Count - 1;

        public Subject<Unit> PointReached { get; } = new Subject<Unit>();

        private List<PlayerRef> _playersInsideCartZone = new List<PlayerRef>();
        private TeamsService _teamsService;

        private float _pushTime;
        
        [Inject]
        private void Construct(TeamsService teamsService)
        {
            _teamsService = teamsService;
        }
        
        [ContextMenu(nameof(UpdatePath))]
        private void UpdatePath()
        {
            _pathPoints = GetComponentsInChildren<CartPathPoint>().ToList();
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            PlayersSpawner playersSpawner = FindObjectOfType<PlayersSpawner>();
            playersSpawner.PlayerBaseDeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerLeft));

            _cart.CartZoneEntered.TakeUntilDestroy(this).Subscribe((OnCartZoneEntered));
            _cart.CartZoneExit.TakeUntilDestroy(this).Subscribe((OnCartZoneExit));

            HighlightControlPoints();
            
            ResetCart();
        }
        
        private void OnCartZoneEntered(PlayerRef playerRef)
        {
            _playersInsideCartZone.Add(playerRef);

            AllowToMove = !IsCartBlocked();
        }

        private void OnCartZoneExit(PlayerRef playerRef)
        {
            _playersInsideCartZone.Remove(playerRef);

            AllowToMove = !IsCartBlocked();

            if (_playersInsideCartZone.Count == 0)
            {
                AllowToMove = false;
            }
        }
        
        private void OnPlayerLeft(PlayerRef playerRef)
        {
            if (_playersInsideCartZone.Contains(playerRef))
            {
                OnCartZoneExit(playerRef);
            }
        }

        private void HighlightControlPoints()
        {
            foreach (CartPathPoint pathPoint in _pathPoints)
            {
                if (pathPoint.IsControlPoint)
                {
                    pathPoint.transform.localScale = Vector3.one * 3f;
                }
                else
                {
                    pathPoint.View.gameObject.SetActive(false);
                }
            }
        }

        private void InitCart()
        {
            _cart.transform.position = _currentPoint.transform.position;
            _cart.transform.Rotate2D(_nextPoint.transform.position);
        }

        public void ResetCart()
        {
            _currentPointIndex = 0;

            _currentPoint = _pathPoints[_currentPointIndex];
            _nextPoint = _pathPoints[_currentPointIndex + 1];

            InitCart();
        }

        public override void FixedUpdateNetwork()
        {
            if (_nextPoint == null) return;

            if (_currentPoint == null) return;

            if (AllowToMove == false) return;

            MoveCartAlongPoints();

            var distance = Vector3.Distance(_cart.transform.position, _nextPoint.transform.position);

            if (distance < 0.5)
            {
                SetNewPoints(_currentPointIndex);
            }
        }

        private void MoveCartAlongPoints()
        {
            Vector3 direction = _nextPoint.transform.position - _currentPoint.transform.position;
            direction.Normalize();

            float speed = _cartVelocity * Runner.DeltaTime;
            
            _cart.transform.position += direction * speed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (angle < 0)
            {
                angle += 360;
            }

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            _cart.transform.rotation = Quaternion.Lerp(_cart.transform.rotation, targetRotation,
                Runner.DeltaTime * _cartRotationSpeed);
        }

        private void SetNewPoints(int currentPointIndex)
        {
            currentPointIndex++;
            _prevPoint = _currentPoint;
            _currentPoint = _nextPoint;

            if (currentPointIndex > _pathPoints.Count - 1)
            {
                _nextPoint = null;
            }
            else
            {
                _nextPoint = _pathPoints[currentPointIndex];
            }

            _currentPointIndex = currentPointIndex;

            if (_currentPoint.IsControlPoint)
            {
                PointReached.OnNext(Unit.Default);
            }
        }

        private bool IsCartBlocked()
        {
            foreach (var playerRef in _playersInsideCartZone)
            {
                TeamSide playerTeamSide = _teamsService.GetUnitTeamSide(playerRef);

                if (playerTeamSide != _teamToCapturePoints) return true;
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            for (var i = 0; i < _pathPoints.Count; i++)
            {
                if (i == _pathPoints.Count - 1)
                {
                    return;
                }

                var cartPathPoint = _pathPoints[i];
                var cartPathPointNext = _pathPoints[i + 1];
                Gizmos.DrawLine(cartPathPoint.transform.position, cartPathPointNext.transform.position);
            }
        }
    }
}