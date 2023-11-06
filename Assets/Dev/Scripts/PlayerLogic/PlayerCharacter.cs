using Dev.Infrastructure;
using Dev.Weapons;
using Dev.Weapons.Guns;
using Fusion;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerCharacter : NetworkContext, IDamageable
    {
        [Range(0f, 1f)] [SerializeField] private float _shootThreshold = 0.75f;
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private HitboxRoot _hitboxRoot;
        [SerializeField] private WeaponController _weaponController;

        [SerializeField] private PlayerController _playerController;

        [SerializeField] private Rigidbody2D _rigidbody2D;
        
        public PlayerController PlayerController => _playerController;
        public PlayerView PlayerView => _playerView;
        public HitboxRoot HitboxRoot => _hitboxRoot;
        public Rigidbody2D Rigidbody => _rigidbody2D;
        public WeaponController WeaponController => _weaponController;

        public float ShootThreshold => _shootThreshold;

        [Networked] public CharacterClass CharacterClass { get; private set; }

        public static PlayerCharacter LocalPlayerCharacter;

        public override void Spawned()
        {
            if (HasInputAuthority)
            {
                LocalPlayerCharacter = this;
            }
            
        }

        public void Init(CharacterClass characterClass)
        {
            CharacterClass = characterClass;
        }

        public int Id => Object.InputAuthority;
    }
}