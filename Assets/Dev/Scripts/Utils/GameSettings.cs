using UnityEngine;

namespace Dev.Utils
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "StaticData/GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private int _timeAfterWinGame = 5;

        public int TimeAfterWinGame => _timeAfterWinGame;
    }
}