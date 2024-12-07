using Dev.Infrastructure.Networking;
using Dev.Weapons;
using Dev.Weapons.Guns;
using DG.Tweening;
using Fusion;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerCharacter : NetworkContext, IDamageable
    {
        [Range(0f, 1f)] [SerializeField] private float _shootThreshold = 0.75f;
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private Collider2D _collider2D;
        [SerializeField] private WeaponController _weaponController;

        [SerializeField] private Rigidbody2D _rigidbody2D;
        
        public PlayerView PlayerView => _playerView;
        public Collider2D Collider2D => _collider2D;
        public Rigidbody2D Rigidbody => _rigidbody2D;
        public WeaponController WeaponController => _weaponController;
        
        [Networked] private NetworkBool IsDead { get; set; }
        [Networked] public CharacterClass CharacterClass { get; private set; }
        
        public static PlayerCharacter LocalPlayerCharacter;

        public override void Spawned()
        {
            base.Spawned();
            
            if (HasStateAuthority) 
                LocalPlayerCharacter = this;
        }
        
        [Rpc]
        public void RPC_Init(CharacterClass characterClass)
        {
            CharacterClass = characterClass;
        }

        [Rpc]
        public void RPC_OnDeath()
        {
            transform.DOScale(0, 0.5f);
            
            IsDead = true;
            
            _collider2D.enabled = false;
        }
        
        [Rpc]
        public void RPC_ResetAfterDeath()
        {
            transform.DOScale(1, 0);
            
            IsDead = false;
            
            _collider2D.enabled = true;
        }
        
        
        public DamagableType DamageId => DamagableType.Player;


        public static implicit operator PlayerRef(PlayerCharacter playerCharacter)
        {
            return playerCharacter.Object.InputAuthority;
        }
    }
}   