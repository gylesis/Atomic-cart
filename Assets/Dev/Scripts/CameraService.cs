using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using UnityEngine;

namespace Dev
{
    public class CameraService : NetworkContext
    {
        [SerializeField] private Camera _mainCamera;

        public void SetMainCameraState(bool isOn)
        {
            _mainCamera.gameObject.SetActive(isOn);
        }
    }
}