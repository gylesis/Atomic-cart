using Dev.Utils;
using Dev.Weapons.StaticData;
using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "StaticData/GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [Header("Game")]
        [SerializeField] private int _timeAfterWinGame = 5;
        [SerializeField] private bool _isFriendlyFireOn;
        [SerializeField] private bool _saveLogsAfterQuit;
        
        [Header("Bots")] 
        [SerializeField] private BotsConfig _botsConfig;

        [Header("Player")] 
        [SerializeField] private float _cameraZoomModifier = 15f;

        [SerializeField] private float _cameraFollowSpeed = 2f;
        [SerializeField] private LayerMask _weaponObstaclesDetectLayers;
        [SerializeField] private float _weaponHitDetectionOffset = 1;
        [Range(0.1f, 1f)] [SerializeField] private float _shootThreshold = 0.5f;
        
        
        [Header("Weapon")]
        [SerializeField] private WeaponStaticDataContainer _weaponStaticDataContainer;

        [SerializeField] private AnimationCurve _grenadeFlyFunction;
        [SerializeField] private AnimationCurve _grenadeFlySizeFunction;

        [Header("Debug")]
        [SerializeField] private MapName _firstLevelName;
        [SerializeField] private bool _isDebugMode;

        [SerializeField] private float _barrelsRespawnCooldown = 5f;

        public bool IsDebugMode => _isDebugMode;

        public float BarrelsRespawnCooldown => _barrelsRespawnCooldown;
        public BotsConfig BotsConfig => _botsConfig;

        public float Radius = 1;

        public bool SaveLogsAfterQuit => _saveLogsAfterQuit;

        public WeaponStaticDataContainer WeaponStaticDataContainer => _weaponStaticDataContainer;

        public AnimationCurve GrenadeFlyFunction => _grenadeFlyFunction;
        public AnimationCurve GrenadeFlySizeFunction => _grenadeFlySizeFunction;

        public MapName FirstLevelName => _firstLevelName;

        public bool IsFriendlyFireOn => _isFriendlyFireOn;
        public float CameraZoomModifier => _cameraZoomModifier;
        public float CameraFollowSpeed => _cameraFollowSpeed;

        public LayerMask WeaponObstaclesDetectLayers => _weaponObstaclesDetectLayers;

        public float WeaponHitDetectionOffset => _weaponHitDetectionOffset;

        public int TimeAfterWinGame => _timeAfterWinGame;
        public float ShootThreshold => _shootThreshold;
    }
}