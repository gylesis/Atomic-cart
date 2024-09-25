using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(menuName = "StaticData/BotsConfig", fileName = "BotsConfig", order = 0)]
    public class BotsConfig : ScriptableObject
    {
        [SerializeField] private float _botsTargetsSearchRadius = 15;
        [SerializeField] private int _botsPerTeam = 2;
        [SerializeField] private float _botsSearchForTargetsCooldown = 0.25f;
        [SerializeField] private float _botsChangeMoveDirectionCooldown = 5;
        [SerializeField] private int _botsNearestPointsAmountToChoose = 5;
        
        public int BotsPerTeam => _botsPerTeam;
        public float BotsTargetsSearchRadius => _botsTargetsSearchRadius;
        public float BotsSearchForTargetsCooldown => _botsSearchForTargetsCooldown;
        public float BotsChangeMoveDirectionCooldown => _botsChangeMoveDirectionCooldown;
        public int BotsNearestPointsAmountToChoose => _botsNearestPointsAmountToChoose;
    }
}