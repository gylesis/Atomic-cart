using UnityEngine;

namespace Dev
{
    public class CartPathPoint : MonoBehaviour
    {
        [SerializeField] private bool _isControlPoint;

        public bool IsControlPoint => _isControlPoint;
    }
}