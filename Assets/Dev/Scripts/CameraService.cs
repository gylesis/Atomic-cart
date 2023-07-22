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
            Debug.Log($"Awake");

            _playersSpawner = FindObjectOfType<PlayersSpawner>();
        }

        public override void Spawned()
        {
            Debug.Log($"Spawned");

            if (Runner.IsServer)
            {
                _playersSpawner.Spawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            }
        }

        [Rpc]
        private void RPC_SetCameraState([RpcTarget] PlayerRef target, bool isOn)
        {
            _mainCamera.gameObject.SetActive(isOn);
        }

        private void OnPlayerSpawned(PlayerSpawnEventContext spawnEventContext)
        {
            Transform playerTransform = spawnEventContext.Transform;
            PlayerRef playerRef = spawnEventContext.PlayerRef;

            RPC_SetCameraState(playerRef, false);

            CameraController cameraController = Runner.Spawn(_cameraControllerPrefab, playerTransform.position,
                Quaternion.identity,
                playerRef);

            cameraController.SetupTarget(playerTransform);
        }
    }
}