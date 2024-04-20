using UnityEngine;

namespace Dev.Infrastructure
{
    public class SelectionInfoContainer : MonoBehaviour
    {
        [SerializeField] private GameObject _onParent;
        [SerializeField] private GameObject _offParent;

        public GameObject OnParent => _onParent;

        public GameObject OffParent => _offParent;
    }
}