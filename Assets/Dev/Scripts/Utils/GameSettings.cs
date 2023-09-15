using UnityEngine;

namespace Dev.Utils
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "StaticData/GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [Header("Game")]
        [SerializeField] private int _timeAfterWinGame = 5;

        [Header("Player")] 
        [SerializeField] private float _cameraZoomModifier = 15f;


        public float CameraZoomModifier => _cameraZoomModifier;

        public int TimeAfterWinGame => _timeAfterWinGame;
    }
}