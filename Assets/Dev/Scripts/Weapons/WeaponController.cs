using System.Linq;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Weapons.Guns;
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

        private Player _player;
        private WeaponProvider _weaponProvider;

        public bool AllowToShoot { get; private set; } = true;

        [HideInInspector]
        [Networked]
        [CanBeNull]
        public Weapon CurrentWeapon { get; set; }

        public Subject<Weapon> WeaponChanged { get; } = new Subject<Weapon>();

        public override void Spawned()
        {
            if (HasInputAuthority)
            {
                Weapon weapon = WeaponParent.GetComponentInChildren<Weapon>();

                if (weapon != null)
                {
                    RPC_AddWeapon(weapon, true);
                }
            }

            if (HasStateAuthority == false) return;

            foreach (Weapon weapon in Weapons)
            {
                weapon.transform.parent = WeaponParent;
            }

            _weaponProvider = new WeaponProvider(_weaponStaticDataContainer, Runner);
        }

        public void Init(WeaponSetupContext weaponSetupContext)
        {
            _weaponProvider.ProvideWeaponToPlayer(Object.InputAuthority, weaponSetupContext.Name.Value, true);
        }

        public bool HasAnyWeapon => Weapons.Count > 0 && CurrentWeapon != null;

        public bool HasWeapon(string name)
        {
            Weapon weapon = Weapons.FirstOrDefault(x => x.WeaponData.Name == name);

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
                var shootDelay = CurrentWeapon.ShootDelay;

                if (shootDelay == 0)
                {
                    Shoot(direction);
                }
            }
        }

        public void TryToFire()
        {
            AllowToShoot = CurrentWeapon.AllowToShoot;

            if (AllowToShoot)
            {
                var shootDelay = CurrentWeapon.ShootDelay;

                if (shootDelay == 0)
                {
                    Shoot();
                }
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
            if(HasStateAuthority == false) return;
            
            var cooldown = CurrentWeapon.Cooldown;

            //Debug.Log($"Power {power}");

            CurrentWeapon.Shoot(direction, power);
            CurrentWeapon.CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);
            CurrentWeapon.ShootDelayTimer = TickTimer.None;

            //_weaponUiView.ShootReloadView(cooldown, cooldown);
        }

        private void Shoot()
        {
            if(HasStateAuthority == false) return;
            
            var cooldown = CurrentWeapon.Cooldown;

            //Debug.Log($"Power {power}");

            CurrentWeapon.Shoot(CurrentWeapon.ShootDirection, 1);
            CurrentWeapon.CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);
            CurrentWeapon.ShootDelayTimer = TickTimer.None;

            //_weaponUiView.ShootReloadView(cooldown, cooldown);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                /*var hasInput = GetInput<PlayerInput>(out var input);

                if (hasInput)
                {
                    if (input.WeaponNum != 22)
                    {
                        if (input.WeaponNum == 1)
                        {
                            _weaponProvider.ProvideWeaponToPlayer<AkWeapon>(Object.InputAuthority, true);
                        }
                        else if (input.WeaponNum == 2)
                        {
                            _weaponProvider.ProvideWeaponToPlayer<BazookaWeapon>(Object.InputAuthority, true);
                        }
                    }
                }*/
            }


            if (HasInputAuthority == false) return;

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
                Debug.Log($"Weapon already chosen");
                return;
            }

            CurrentWeapon = chosenWeapon;
            CurrentWeapon.OnChosen();

            WeaponChanged.OnNext(CurrentWeapon);
            OnWeaponChanged(chosenWeapon);

            RPC_SelectViewWeapon();
        }

        [Rpc]
        public void RPC_ChooseWeapon(string weaponName)
        {
            if (Weapons.Count == 0)
            {
                return;
            }

            Weapon weapon = Weapons.First(x => x.WeaponData.Name == weaponName);

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