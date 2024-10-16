using System;
using Dev.Weapons.Guns;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Dev.Weapons.StaticData
{
    public abstract class WeaponStaticData : ScriptableObject
    {
        [SerializeField] private WeaponType _weaponType;
        [SerializeField] private Weapon _weaponPrefab;

        [SerializeField] protected float _bulletMaxDistance = 10f;
        [SerializeField] protected float _cooldown = 1f;
        [SerializeField] private int _minDamage = 8;
        [SerializeField] private int _maxDamage = 12;

        [SerializeField] private string _fireSound = "ak_fire";
        [SerializeField] private string _hitSound = "ak_hit";

        public string FireSound => _fireSound;
        public string HitSound => _hitSound;

        public Type SystemType => _weaponPrefab.GetType();
        
        public string WeaponName => _weaponType.ToString();
        public Weapon Prefab => _weaponPrefab;
        public WeaponType WeaponType => _weaponType;
        public float BulletMaxDistance => _bulletMaxDistance;

        public float Cooldown => _cooldown;

        public int Damage => Random.Range(_minDamage, _maxDamage + 1);

    }
    
}