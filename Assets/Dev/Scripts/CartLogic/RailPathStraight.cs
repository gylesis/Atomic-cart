using UnityEngine;

namespace Dev.CartLogic
{
    public class RailPathStraight : MonoBehaviour
    {
        [SerializeField] private Transform _secondBone;

        public Transform SecondBone => _secondBone;
    }
}