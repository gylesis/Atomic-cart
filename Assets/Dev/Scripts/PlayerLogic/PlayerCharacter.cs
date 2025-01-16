using Dev.Infrastructure;
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
        
        [Networked] private NetworkBool IsAlive { get; set; } = true;
        [Networked] public CharacterClass CharacterClass { get; set; }
        
        public static PlayerCharacter LocalPlayerCharacter;

        protected override void CorrectState()
        {
            base.CorrectState();
            
            if(!HasStateAuthority)
                UpdateViewForClass(CharacterClass);
        }

        public override void Spawned()
        {
            base.Spawned();
            
            if (HasStateAuthority) 
                LocalPlayerCharacter = this;
        }
        
        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_Init(CharacterClass characterClass)
        {
            CharacterClass = characterClass;
            UpdateViewForClass(characterClass);
        }

        private void UpdateViewForClass(CharacterClass characterClass)
        {
            CharacterData characterData = GameSettingsProvider.GameSettings.CharactersDataContainer.GetCharacterDataByClass(characterClass);
            _playerView.UpdateView(characterData.AnimatorController, characterData.CharacterSprite);
        }

        public void SetAliveState(bool isAlive)
        {
            RPC_DoScale(0.3f, isAlive ? 1 : 0);
            RPC_OnAliveChanged(isAlive);
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_OnAliveChanged(bool isAlive)
        {
            IsAlive = isAlive;
            _collider2D.enabled = isAlive;
        }
        
        public DamagableType DamageId => DamagableType.Player;

        public static implicit operator PlayerRef(PlayerCharacter playerCharacter)
        {
            return playerCharacter.Object.InputAuthority;
        }
    }
}   