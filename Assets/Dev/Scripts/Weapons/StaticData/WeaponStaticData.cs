using System;
using Dev.Weapons.Guns;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Dev.Weapons.StaticData
{
    public abstract class WeaponStaticData : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private WeaponType _weaponType;
        [SerializeField] private Weapon _weaponPrefab;

        [SerializeField] protected float _bulletMaxDistance = 10f;
        [SerializeField] protected float _cooldown = 1f;
        [SerializeField] private int _minDamage = 8;
        [SerializeField] private int _maxDamage = 12;

        [Header("Effects")]
        [SerializeField] private string _fireSound = "ak_fire";
        [SerializeField] private string _hitSound = "ak_hit";
        [SerializeField] private string _shakePatternKey = "TO IMPLEMENT";
        [SerializeField] private string _shellsKey = "gun_shells";
        [SerializeField] private string _muzzleKey = "TO IMPLEMENT";

        public string FireSound => _fireSound;
        public string HitSound => _hitSound;

        public string ShakePatternKey => _shakePatternKey;
        public string ShellsKey => _shellsKey;
        public string MuzzleKey => _muzzleKey;

        public Type SystemType => _weaponPrefab.GetType();
        
        public string WeaponName => _weaponType.ToString();
        public Weapon Prefab => _weaponPrefab;
        public WeaponType WeaponType => _weaponType;
        public float BulletMaxDistance => _bulletMaxDistance;

        public float Cooldown => _cooldown;

        public int Damage => Random.Range(_minDamage, _maxDamage + 1);
    }
}