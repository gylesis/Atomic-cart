using UnityEngine;

namespace Dev.Levels
{
    [CreateAssetMenu(menuName = "StaticData/Levels/LevelsContainer", fileName = "LevelsContainer", order = 0)]
    public class LevelsContainer : ScriptableObject
    {
        [SerializeField] private LevelStaticData[] _levelStaticDatas;

        public LevelStaticData[] LevelStaticDatas => _levelStaticDatas;
    }
}