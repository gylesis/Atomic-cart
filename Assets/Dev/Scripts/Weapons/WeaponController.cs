using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure;
using Dev.Weapons.Guns;
using Dev.Weapons.View;
using Fusion;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public class WeaponController : NetworkContext
    {
        [SerializeField] private List<Weapon> _weapons;

        public int WeaponsAmount => _weapons.Count;

       // private WeaponUiView _weaponUiView;
        private Player _player;

        public bool AllowToShoot { get; set; } = true;
        [Networked] [CanBeNull] public Weapon CurrentWeapon { get; set; }

        public Subject<Weapon> WeaponChanged { get; } = new Subject<Weapon>();

        public override void Spawned()
        {
          //  _weaponUiView = FindObjectOfType<WeaponUiView>();

            if (Object.HasInputAuthority)
            {
                RPC_ChooseWeapon(1);
            }

            RPC_SelectViewWeapon();
        }

        public bool HasWeapon(string name)
        {
            Weapon weapon = _weapons.FirstOrDefault(x => x.WeaponData.Name == name);

            return weapon != null;
        }

        [Rpc]
        public void RPC_AddWeapon(Weapon weapon)
        {
            _weapons.Add(weapon);
        }

        public void TryToFire(Vector2 direction)
        {
            AllowToShoot = CurrentWeapon.AllowToShoot;

            if (AllowToShoot)
            {
                var shootDelay = CurrentWeapon.ShootDelay;

                if (shootDelay == 0)
                {
                    Shoot(direction);
                }
            }
        }

        private void Shoot(Vector2 direction, float power = 1)
        {
            var cooldown = CurrentWeapon.Cooldown;

            //Debug.Log($"Power {power}");

            CurrentWeapon.Shoot(direction, power);
            CurrentWeapon.CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);
            CurrentWeapon.ShootDelayTimer = TickTimer.None;

            //_weaponUiView.ShootReloadView(cooldown, cooldown);
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasInputAuthority == false) return;

            if (CurrentWeapon == null) return;

            if (CurrentWeapon.ShootDelayTimer.IsRunning)
            {
                var power = CurrentWeapon.ShootDelayTimer.RemainingTime(Runner) / CurrentWeapon.ShootDelay;

                var powerValue = 1 - power.Value;
                CurrentWeapon.StartShoot(powerValue);
            }
        }

        private void OnWeaponChanged(Weapon weapon)
        {
            float time;
            var maxCooldown = weapon.Cooldown;

            if (weapon.CooldownTimer.ExpiredOrNotRunning(Runner) == false)
            {
                var remainingTime = weapon.CooldownTimer.RemainingTime(Runner);
                time = remainingTime.Value;
            }
            else
            {
                time = 0;
            }

            //_weaponUiView.ShootReloadView(time, maxCooldown);
        }

        
        [Rpc]
        public void RPC_ChooseWeapon(int index)
        {
            if (_weapons.Count == 0)
            {
                Debug.Log($"No weapons to choose");
                return;
            }

            var weaponIndex = Mathf.Clamp(index - 1, 0, _weapons.Count - 1);

            Weapon chosenWeapon = _weapons[weaponIndex];

            if (CurrentWeapon == chosenWeapon)
            {
                Debug.Log($"Weapon already chosen");
                return;
            }

            CurrentWeapon = chosenWeapon;
            CurrentWeapon.OnChosen();

            WeaponChanged.OnNext(CurrentWeapon);
            OnWeaponChanged(chosenWeapon);

            RPC_SelectViewWeapon();

            if (Object.HasInputAuthority)
            {
                Debug.Log($"Chosen weapon is {chosenWeapon.name}");
            }
        }

        [Rpc]
        private void RPC_SelectViewWeapon()
        {
            foreach (Weapon weapon in _weapons)
            {
                if (weapon == CurrentWeapon)
                {
                    weapon.SetViewState(true);
                    continue;
                }

                weapon.SetViewState(false);
            }
        }

        /*public void TryToFireClickedUp(Vector3 direction)
        {
            var shootDelay = CurrentWeapon.ShootDelay;

            if (shootDelay == 0) return;

            AllowToShoot = CurrentWeapon.CooldownTimer.ExpiredOrNotRunning(Runner);

            if (AllowToShoot)
            {
                var power = CurrentWeapon.ShootDelayTimer.RemainingTime(Runner) / shootDelay;

                Shoot(direction, 1 - power.Value);
            }
        }*/
    }
}