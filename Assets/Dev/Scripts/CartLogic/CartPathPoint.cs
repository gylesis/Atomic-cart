using UnityEngine;

namespace Dev
{
    public class CartPathPoint : MonoBehaviour
    {
        [SerializeField] private bool isControlPoint;

        public bool IsControlPoint()
        {
            return isControlPoint;
        }

    }
}