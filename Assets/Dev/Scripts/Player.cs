using System;
using Dev.Infrastructure;
using Dev.Weapons;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class Player : NetworkContext
    {
        [SerializeField] private float _speed;
        [Range(0f, 1f)] [SerializeField] private float _shootThreshold = 0.75f;
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] private HitboxRoot _hitboxRoot;
        [SerializeField] private WeaponController _weaponController;

        [SerializeField] private PlayerController _playerController;

        public PlayerController PlayerController => _playerController;
        public PlayerView PlayerView => _playerView;
        public HitboxRoot HitboxRoot => _hitboxRoot;
        public Rigidbody2D Rigidbody => _networkRigidbody2D.Rigidbody;
        public WeaponController WeaponController => _weaponController;

        public float Speed => _speed;
        public float ShootThreshold => _shootThreshold;
    }
}