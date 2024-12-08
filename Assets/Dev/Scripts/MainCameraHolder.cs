using UnityEngine;

namespace Dev
{
    public class MainCameraHolder : MonoContext
    {
        [SerializeField] private Camera _mainCamera;

        public void SetMainCameraState(bool isOn)
        {
            _mainCamera.gameObject.SetActive(isOn);
        }
    }
}