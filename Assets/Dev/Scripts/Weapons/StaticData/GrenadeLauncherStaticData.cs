using UnityEngine;

namespace Dev.Weapons.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Weapons/GrenadeLauncherStaticData", fileName = "GrenadeLauncherStaticData", order = 0)]
    public class GrenadeLauncherStaticData : ProjectileStaticData   
    {
        [SerializeField] private float _grenadeExplosionRadius = 3;
        [SerializeField] private float _grenadeFlyTime = 1;

        [SerializeField] private AnimationCurve _grenadeFlyFunction;
        [SerializeField] private AnimationCurve _grenadeFlySizeFunction;
        
        public AnimationCurve GrenadeFlyFunction => _grenadeFlyFunction;
        public AnimationCurve GrenadeFlySizeFunction => _grenadeFlySizeFunction;
        
        public float GrenadeExplosionRadius => _grenadeExplosionRadius;

        public float GrenadeFlyTime => _grenadeFlyTime;
    }
}