using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Utils
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "StaticData/GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [Header("Game")]
        [SerializeField] private int _timeAfterWinGame = 5;

        [FormerlySerializedAs("_friendlyFireOn")] [SerializeField] private bool _isFriendlyFireOn;
        
        [Header("Player")] 
        [SerializeField] private float _cameraZoomModifier = 15f;


        public bool IsFriendlyFireOn => _isFriendlyFireOn;
        public float CameraZoomModifier => _cameraZoomModifier;

        public int TimeAfterWinGame => _timeAfterWinGame;
    }
}