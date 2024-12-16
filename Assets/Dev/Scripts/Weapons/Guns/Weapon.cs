using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.PlayerLogic;
using Dev.Weapons.StaticData;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Weapons.Guns
{
    public abstract class Weapon : NetworkContext
    {
        [SerializeField] private WeaponType _weaponType;
        
        [SerializeField] protected Transform _shootPoint;
        [SerializeField] protected Transform _view;
        
        public Transform View => _view;
        [Networked] public TickTimer CooldownTimer { get; set; }
        [Networked] public SessionPlayer Owner { get; private set; }
        [Networked] public NetworkBool IsChosen { get; private set; }
            
        public WeaponType WeaponType => _weaponType;
        public virtual bool AllowToShoot => CooldownTimer.ExpiredOrNotRunning(Runner);

        public float BulletMaxDistance => GameSettingsProvider.GameSettings.WeaponStaticDataContainer.GetData(_weaponType).BulletMaxDistance;
        public float Cooldown => GameSettingsProvider.GameSettings.WeaponStaticDataContainer.GetData(_weaponType).Cooldown;
        public int Damage => GameSettingsProvider.GameSettings.WeaponStaticDataContainer.GetData(_weaponType).Damage;

        public Vector2 ShootPos => _shootPoint.position;
        public Transform ShootPoint => _shootPoint;
        public Vector2 ShootDirection => transform.right;

        public float BulletHitOverlapRadius { get; protected set; } = 0.5f;

        protected override void CorrectState()
        {
            base.CorrectState();
            SetViewState(IsChosen);
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_SetOwner(SessionPlayer owner)
        {
            Owner = owner;
        }
        
        public virtual void StartShoot(float power) { }
        
        /// <summary>
        /// Called when shoot button pressed
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="power"></param>
        public abstract void Shoot(Vector2 direction, float power = 1);

        public virtual void Choose(bool chosen) { }
        
        public virtual void SetViewState(bool isActive)
        {
            IsChosen = isActive;
            _view.gameObject.SetActive(IsChosen);
        }
    }
}