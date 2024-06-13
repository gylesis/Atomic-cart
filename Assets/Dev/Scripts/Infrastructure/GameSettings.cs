using Dev.Utils;
using Dev.Weapons.StaticData;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "StaticData/GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [Header("Game")]
        [SerializeField] private int _timeAfterWinGame = 5;

        [SerializeField] private int _botsPerTeam = 2;
        

        [FormerlySerializedAs("_friendlyFireOn")] [SerializeField] private bool _isFriendlyFireOn;
            
        [Header("Player")] 
        [SerializeField] private float _cameraZoomModifier = 15f;
        [SerializeField] private float _cameraFollowSpeed = 2f;
        [SerializeField] private LayerMask _weaponObstaclesDetectLayers;
        [SerializeField] private float _weaponHitDetectionOffset = 1;
        
        [Header("Weapon")]
        [SerializeField] private WeaponStaticDataContainer _weaponStaticDataContainer;
        [SerializeField] private AnimationCurve _grenadeFlyFunction;
        [SerializeField] private AnimationCurve _grenadeFlySizeFunction;
        
        [Header("Debug")]
        [SerializeField] private MapName _firstLevelName;


        public int BotsPerTeam => _botsPerTeam;

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
    }
}