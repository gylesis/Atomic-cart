using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(menuName = "StaticData/BotsConfig", fileName = "BotsConfig", order = 0)]
    public class BotsConfig : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private int _botsPerTeam = 2;

        [Header("Attack")]
        [SerializeField] private float _firstSearchRadius = 20f;
        [SerializeField] private float _secondSearchRadius = 30f;
        [SerializeField] private float _searchForTargetsCooldown = 0.25f;
        [SerializeField] private float _shootRandomnessFactor = 1.5f;
        
        [Header("Movement")]
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _speedWhenAttacking = 3f;
        [SerializeField] private float _changeMoveDirectionCooldown = 5;
        [SerializeField] private int _nearestPointsToChooseAmount = 5;
        [SerializeField] private int _patrolPointsPoolAmount = 5;

        public float Speed => _speed;
        public float SpeedWhenAttacking => _speedWhenAttacking;

        public float ShootRandomnessFactor => _shootRandomnessFactor;

        public int PatrolPointsPoolAmount => _patrolPointsPoolAmount;
        public int BotsPerTeam => _botsPerTeam;
        public float FirstSearchRadius => _firstSearchRadius;
        public float SecondSearchRadius => _secondSearchRadius;
        public float SearchForTargetsCooldown => _searchForTargetsCooldown;
        public float ChangeMoveDirectionCooldown => _changeMoveDirectionCooldown;
        public int NearestPointsToChooseAmount => _nearestPointsToChooseAmount;
    }
}