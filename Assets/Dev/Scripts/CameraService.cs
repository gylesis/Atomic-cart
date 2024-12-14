using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class CameraService : NetSingleton<CameraService>
    {
        [SerializeField] private Transform _testPoint;
        
        private CameraController _cameraInstance;

        private CameraController _cameraControllerPrefab;
        private PlayersSpawner _playersSpawner;
        private MainCameraHolder _mainCameraHolder;
        private SessionStateService _sessionStateService;

        [Inject]
        private void Construct(CameraController cameraControllerPrefab, PlayersSpawner playersSpawner, MainCameraHolder mainCameraHolder, SessionStateService sessionStateService)
        {
            _sessionStateService = sessionStateService;
            _mainCameraHolder = mainCameraHolder;
            _playersSpawner = playersSpawner;
            _cameraControllerPrefab = cameraControllerPrefab;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            _playersSpawner.CharacterSpawned.Subscribe(OnCharacterSpawned).AddTo(GlobalDisposable.SceneScopeToken);
        }

        /*private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                ShakeIfNeed(_testPoint.position, "small_explosion", _sessionStateService.GetSessionPlayer(PlayerRef.MasterClient));
            }
        }*/

        public void ShakeIfNeed(string key, Vector3 position, bool isShakeFromBot, bool isGlobal = false)
        {
            if(isShakeFromBot) return;
            
            foreach (var sessionPlayer in _sessionStateService.Players)
            {
                if(sessionPlayer.IsBot) continue;
                
                RPC_ShakeIfNeed(position, key);
            }
        }


        [Rpc]
        private void RPC_ShakeIfNeed(Vector3 position, string key)
        {
            if(_cameraInstance == null) return;
            
            bool isShakeNeeded = _cameraInstance.Camera.IsPointInCameraView(position, 0.1f);

            if (isShakeNeeded == false)
                return;
            
            _cameraInstance.Shake(key);
        }
        
        private void OnCharacterSpawned(PlayerSpawnEventContext context)
        {
            PlayerRef playerRef = context.PlayerRef;
            if(playerRef != FindObjectOfType<NetworkRunner>().LocalPlayer) return; // TODO
         
            if(_cameraInstance == null)
                SpawnCameraForPlayer(playerRef);
            else
                UpdateCameraForPlayer(playerRef);
        }

        private void UpdateCameraForPlayer(PlayerRef playerRef)
        {
            var playerCharacter = _playersSpawner.GetPlayer(playerRef);

            _cameraInstance.SetupTarget(playerCharacter.transform);
            _cameraInstance.FastSetOnTarget();
            _cameraInstance.SetFollowState(true);
        }

        private void SpawnCameraForPlayer(PlayerRef playerRef)
        {
            _mainCameraHolder.SetMainCameraState(false);
            _cameraInstance = Instantiate(_cameraControllerPrefab);
            
            DiInjecter.Instance.InjectGameObject(_cameraInstance.gameObject);
            
            UpdateCameraForPlayer(playerRef);
        }
    }
}