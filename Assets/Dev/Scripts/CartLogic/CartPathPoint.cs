using UnityEngine;

namespace Dev.CartLogic
{
    public class CartPathPoint : MonoBehaviour
    {
        [SerializeField] private Transform _view;
        [SerializeField] private bool _isControlPoint;

        public bool IsControlPoint => _isControlPoint;
        public Transform View => _view;
    }
}