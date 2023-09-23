using UnityEngine;

namespace Dev.Levels
{
    [CreateAssetMenu(menuName = "StaticData/Levels/LevelStaticData", fileName = "LevelStaticData", order = 0)]
    public class LevelStaticData : ScriptableObject
    {
        public Level Prefab;
        
    }
}