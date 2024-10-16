using UnityEngine;

namespace Dev.Weapons.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Weapons/BazookaStaticData", fileName = "BazookaStaticData", order = 0)]
    public class BazookaStaticData : ProjectileStaticData
    {
        [SerializeField] private float _explosionRadius = 2;
        [SerializeField] private float _firePushPower = 5;

        public float ExplosionRadius => _explosionRadius;

        public float FirePushPower => _firePushPower;
    }
    
}