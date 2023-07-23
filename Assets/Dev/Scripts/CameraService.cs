using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev
{
    public class CameraService : NetworkContext
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private CameraController _cameraControllerPrefab;

        private PlayersSpawner _playersSpawner;

        private void Awake()
        {
            _playersSpawner = FindObjectOfType<PlayersSpawner>();
            _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
        }

        public void RPC_SetMainCameraState(bool isOn)
        {
            _mainCamera.gameObject.SetActive(isOn);
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            Transform playerTransform = spawnEventContext.Transform;
            PlayerRef playerRef = spawnEventContext.PlayerRef;

            if (Runner.IsServer)
            {
                CameraController cameraController = Runner.Spawn(_cameraControllerPrefab, playerTransform.position,
                    Quaternion.identity,
                    playerRef);

               // RPC_SpawnCamera(playerRef);
            }
        }

    }
}