using System.Linq;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Weapons.Guns;
using Dev.Weapons.StaticData;
using Fusion;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public class WeaponController : NetworkContext
    {
        [SerializeField] private WeaponStaticDataContainer _weaponStaticDataContainer;

        [Networked, Capacity(4)] private NetworkLinkedList<Weapon> Weapons { get; }

        [SerializeField] private Transform _weaponParent;

        public Transform WeaponParent => _weaponParent;

        public int WeaponsAmount => Weapons.Count;

        private PlayerCharacter _playerCharacter;
        private WeaponProvider _weaponProvider;

        public bool AllowToShoot { get; private set; } = true;

        [HideInInspector]
        [Networked]
        [CanBeNull]
        public Weapon CurrentWeapon { get; set; }

        public Subject<Weapon> WeaponChanged { get; } = new Subject<Weapon>();

        public override void Spawned()
        {
            if (HasStateAuthority == false) return;

            Weapon weapon = WeaponParent.GetComponentInChildren<Weapon>();

            if (weapon != null)
            {
                RPC_AddWeapon(weapon, true);
            }
            
            foreach (Weapon weap in Weapons)
            {
                weap.transform.parent = WeaponParent;
            }

            _weaponProvider = new WeaponProvider(_weaponStaticDataContainer, Runner);
        }

        public void Init(WeaponSetupContext weaponSetupContext)
        {
            _weaponProvider.ProvideWeaponToPlayer(Object.InputAuthority, weaponSetupContext.WeaponType, true);
        }

        public bool HasAnyWeapon => Weapons.Count > 0 && CurrentWeapon != null;

        public bool HasWeapon(WeaponType weaponType)
        {
            Weapon weapon = Weapons.FirstOrDefault(x => x.WeaponType == weaponType);

            return weapon != null;
        }

        [Rpc]
        public void RPC_AddWeapon(Weapon weapon, bool withChoose = false)
        {
            Weapons.Add(weapon);

            weapon.transform.parent = WeaponParent;

            if (withChoose)
            {
                RPC_ChooseWeapon(Weapons.Count);
            }
        }

        public void TryToFire(Vector2 direction)
        {
            AllowToShoot = CurrentWeapon.AllowToShoot;

            if (AllowToShoot)
            {
                Shoot(direction);
            }
        }

        public void TryToFire()
        {
            AllowToShoot = CurrentWeapon.AllowToShoot;

            if (AllowToShoot)
            {
                Shoot();
            }
        }

        public void AimWeaponTowards(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (angle < 0)
            {
                angle += 360;
            }

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            WeaponParent.transform.up = direction;
        }

        private void Shoot(Vector2 direction, float power = 1)
        {
            //if (HasStateAuthority == false) return;

            var cooldown = CurrentWeapon.Cooldown;

            //Debug.Log($"Power {power}");

            CurrentWeapon.Shoot(direction, power);
            CurrentWeapon.CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);

            //_weaponUiView.ShootReloadView(cooldown, cooldown);
        }

        private void Shoot()
        {
            if (HasStateAuthority == false) return;

            var cooldown = CurrentWeapon.Cooldown;

            //Debug.Log($"Power {power}");

            CurrentWeapon.Shoot(CurrentWeapon.ShootDirection, 1);
            CurrentWeapon.CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);

            //_weaponUiView.ShootReloadView(cooldown, cooldown);
        }

        
        private void OnWeaponChanged(Weapon weapon)
        {
            return;
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
            if (Weapons.Count == 0)
            {
                return;
            }

            var weaponIndex = Mathf.Clamp(index - 1, 0, Weapons.Count - 1);

            Weapon chosenWeapon = Weapons[weaponIndex];

            if (CurrentWeapon == chosenWeapon)
            {
               // Debug.Log($"Weapon already chosen");
                return;
            }

            CurrentWeapon = chosenWeapon;
            CurrentWeapon.OnChosen();

            WeaponChanged.OnNext(CurrentWeapon);
            OnWeaponChanged(chosenWeapon);

            RPC_SelectViewWeapon();
        }

        [Rpc]
        public void RPC_ChooseWeapon(WeaponType weaponType)
        {
            if (Weapons.Count == 0)
            {
                return;
            }

            Weapon weapon = Weapons.First(x => x.WeaponType == weaponType);

            if (CurrentWeapon == weapon)
            {
                Debug.Log($"Weapon already chosen");
                return;
            }

            CurrentWeapon = weapon;
            CurrentWeapon.OnChosen();

            WeaponChanged.OnNext(CurrentWeapon);
            OnWeaponChanged(weapon);

            RPC_SelectViewWeapon();
        }


        [Rpc]
        private void RPC_SelectViewWeapon()
        {
            foreach (Weapon weapon in Weapons)
            {
                if (weapon == CurrentWeapon)
                {
                    weapon.SetViewState(true);
                    continue;
                }

                weapon.SetViewState(false);
            }
        }
    }
}