using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(menuName = "StaticData/LevelEnvironmentConfig", fileName = "LevelEnvironmentConfig", order = 0)]
    public class LevelEnvironmentConfig : ScriptableObject      
    {
        [SerializeField] private CloudsConfig _cloudsConfig;

        public CloudsConfig CloudsConfig => _cloudsConfig;
    }
}