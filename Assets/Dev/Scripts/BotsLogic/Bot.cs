using Dev.Infrastructure;
using UnityEngine;

namespace Dev.BotsLogic
{
    public class Bot : NetworkContext
    {
        [SerializeField] private Transform[] _movePoints;
        [SerializeField] private float _speed = 1.2f;
        
        private int _currentPointIndex = 0; 
      
        public override void FixedUpdateNetwork()
        {
            Move(Runner.DeltaTime);
        }

        private void Move(float delta)
        {
            Transform movePoint = _movePoints[_currentPointIndex];

            var distance = Vector3.Distance(movePoint.position, transform.position);

            if (distance < 1)
            {
                _currentPointIndex++;

                if (_currentPointIndex == _movePoints.Length)
                {
                    _currentPointIndex = 0;
                }
                
            }

            transform.position = Vector3.MoveTowards(transform.position, movePoint.position, delta * _speed);
        }
    }
}