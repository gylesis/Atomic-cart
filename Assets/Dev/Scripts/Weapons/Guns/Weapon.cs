using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    public abstract class Weapon : NetworkContext
    {
        [SerializeField] protected Transform _shootPoint;
        [SerializeField] protected Transform _view;

        [SerializeField] protected float _cooldown = 1f;
        [SerializeField] protected int _damage = 10;
        [SerializeField] protected float _shootDelay = 0;
        [Networked] public TickTimer CooldownTimer { get; set; }
        [Networked] public TickTimer ShootDelayTimer { get; set; }
        
       
        public virtual bool AllowToShoot => CooldownTimer.ExpiredOrNotRunning(Runner);

        public float Cooldown => _cooldown;
        public int Damage => _damage;
        public float ShootDelay => _shootDelay;

        public Vector2 ShootPos => _shootPoint.position;
        public Transform ShootPoint => _shootPoint;

        public Vector2 ShootDirection => transform.up;
        public WeaponData WeaponData { get; private set; }

        public void Init(WeaponData weaponData)
        {
            WeaponData = weaponData;
        }

        public virtual void StartShoot(float power) { }
        public abstract void Shoot(Vector2 direction, float power = 1);

        public virtual void OnChosen() { }

        public virtual void SetViewState(bool isActive)
        {
            _view.gameObject.SetActive(isActive);
        }
    }
    
    
}