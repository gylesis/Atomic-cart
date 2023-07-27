using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Unity.VisualScripting;
using UnityEngine;

namespace Dev
{
    public class CartPathService:NetworkContext
    {
        [SerializeField] private Cart _cart;
        [SerializeField] private List<CartPathPoint> _pathPoints;
        Vector3 startCartPosition => _pathPoints.First().transform.position;
        private CartPathPoint _currentPoint;
        private CartPathPoint _nextPoint;
        [SerializeField] private float _cartVelocity = 2f;
        private int currentPointIndex = 0;

        public override void Spawned()
        {
            if (HasStateAuthority == false) return;
            _currentPoint = _pathPoints[currentPointIndex];
            _nextPoint = _pathPoints[currentPointIndex+1];
            _cart.transform.position = _currentPoint.transform.position;
            
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
                var cartPathPointNext = _pathPoints[i+1];
                Gizmos.DrawLine(cartPathPoint.transform.position, cartPathPointNext.transform.position);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;
            Vector3 cartPosition = _cart.transform.position;
            Vector3 direction = _nextPoint.transform.position - _currentPoint.transform.position;
            direction.Normalize();
            cartPosition += direction * _cartVelocity * Runner.DeltaTime;
            _cart.transform.position = cartPosition;


            var distance = Vector3.Distance(cartPosition, _nextPoint.transform.position);
            if (distance<0.1)
            {
                currentPointIndex++;
                _currentPoint = _nextPoint;
                _nextPoint = _pathPoints[currentPointIndex];
                
            }
        }
    }
}

