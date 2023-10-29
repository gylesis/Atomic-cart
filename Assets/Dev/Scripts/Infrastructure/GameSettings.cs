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

        [FormerlySerializedAs("_friendlyFireOn")] [SerializeField] private bool _isFriendlyFireOn;
            
        [Header("Player")] 
        [SerializeField] private float _cameraZoomModifier = 15f;

        [Header("Weapon")]
        [SerializeField] private WeaponStaticDataContainer _weaponStaticDataContainer;
        [SerializeField] private AnimationCurve _grenadeFlyFunction;
        [SerializeField] private AnimationCurve _grenadeFlySizeFunction;
        
        [Header("Debug")]
        [SerializeField] private MapName _firstLevelName;

        public WeaponStaticDataContainer WeaponStaticDataContainer => _weaponStaticDataContainer;

        public AnimationCurve GrenadeFlyFunction => _grenadeFlyFunction;
        public AnimationCurve GrenadeFlySizeFunction => _grenadeFlySizeFunction;

        public MapName FirstLevelName => _firstLevelName;

        public bool IsFriendlyFireOn => _isFriendlyFireOn;
        public float CameraZoomModifier => _cameraZoomModifier;

        public int TimeAfterWinGame => _timeAfterWinGame;
    }
}