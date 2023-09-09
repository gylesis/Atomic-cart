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
        [SerializeField] private int _minDamage = 8;
        [SerializeField] private int _maxDamage = 12;
        [SerializeField] protected float _shootDelay = 0;
        [Networked] public TickTimer CooldownTimer { get; set; }
        [Networked] public TickTimer ShootDelayTimer { get; set; }


        public virtual bool AllowToShoot => CooldownTimer.ExpiredOrNotRunning(Runner);

        public float Cooldown => _cooldown;
        public int Damage => Random.Range(_minDamage, _maxDamage + 1);
        public float ShootDelay => _shootDelay;

        public Vector2 ShootPos => _shootPoint.position;
        public Transform ShootPoint => _shootPoint;

        public Vector2 ShootDirection => transform.up;

        [Networked] public WeaponData WeaponData { get; private set; }

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