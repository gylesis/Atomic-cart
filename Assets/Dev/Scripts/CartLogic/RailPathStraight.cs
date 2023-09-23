using UnityEngine;

namespace Dev
{
    public class RailPathStraight : MonoBehaviour
    {
        [SerializeField] private Transform _secondBone;

        public Transform SecondBone => _secondBone;
    }
}