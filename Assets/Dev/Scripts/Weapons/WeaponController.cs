using System.Linq;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.PlayerLogic;
using Dev.Utils;
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
        [SerializeField] private Transform _weaponParent;

        private PlayerCharacter _playerCharacter;
       // private WeaponProvider _weaponProvider;
       
        public bool AllowToShoot { get; private set; } = true;
        [HideInInspector, Networked] public Weapon CurrentWeapon { get; set; }
        [Networked, Capacity(4)] private NetworkLinkedList<Weapon> Weapons { get; }
        [Networked] public SessionPlayer Owner { get; private set; }
        [Networked] public NetworkBool TeamWasSet { get; private set; }
        public Transform WeaponParent => _weaponParent;
        public int WeaponsAmount => Weapons.Count;
        public Vector3 Direction => WeaponParent.up;
        public bool HasAnyWeapon => Weapons.Count > 0 && CurrentWeapon != null;
        public Subject<Weapon> WeaponChanged { get; } = new Subject<Weapon>();

        public void Init(WeaponSetupContext weaponSetupContext)
        {
            //_weaponProvider.ProvideWeaponToPlayer(Object.InputAuthority, weaponSetupContext.WeaponType, true);
        }

        public override void Spawned()
        {
            base.Spawned();
            
            if (HasStateAuthority == false) return;

            var weapons = WeaponParent.GetComponentsInChildren<Weapon>();
            int randomWeapon = Random.Range(0, weapons.Length + 1);

            for (var index = 0; index < weapons.Length; index++)
            {
                var weapon = weapons[index];
                RPC_AddWeapon(weapon, index == randomWeapon);
            }

            foreach (Weapon weap in Weapons) 
                weap.transform.parent = WeaponParent;

            //_weaponProvider = new WeaponProvider(_weaponStaticDataContainer, Runner);
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_SetOwner(SessionPlayer owner)
        {
            Owner = owner;
            TeamWasSet = true;
        }

        public bool HasWeapon(WeaponType weaponType)
        {
            Weapon weapon = Weapons.FirstOrDefault(x => x.WeaponType == weaponType);

            return weapon != null;
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        public void RPC_AddWeapon(Weapon weapon, bool withChoose = false)
        {
            Weapons.Add(weapon);

            weapon.transform.parent = WeaponParent;

            weapon.RPC_SetOwner(Owner);

            if (withChoose) 
                ChooseWeapon(Weapons.Count);
        }

        public void TryToFire(Vector2 direction)
        {
            if (AbleToShoot() == false) return;

            Shoot(direction);
        }

        public void TryToFire()
        {
            if (AbleToShoot() == false) return;

            Shoot();
        }

        public void AimWeaponTowards(Vector2 direction)
        {
            WeaponParent.RotateTowardsDirection(direction);
            
            float scaleSign = 1;

            if (direction.x < 0) 
                scaleSign = -1;

            var localScale = CurrentWeapon.View.localScale;
            localScale.y = scaleSign;

            CurrentWeapon.View.localScale = localScale;
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


        private bool AbleToShoot()
        {
            if (TeamWasSet == false)
            {
                Debug.Log($"Owner of this Weapon Controller is not set, please initialize owner team", gameObject);
                return false;
            }

            if (CurrentWeapon.AllowToShoot == false) return false;

            return true;
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

        public void ChooseWeapon(int index)
        {
            if (Weapons.Count == 0)
            {
                AtomicLogger.Log($"Zero weapons to choose from");
                return;
            }

            var nextWeaponIndex = Mathf.Clamp(index - 1, 0, Weapons.Count - 1);

            Weapon nextWeapon = Weapons[nextWeaponIndex];

            if (CurrentWeapon == nextWeapon)
            {
                Debug.Log($"Weapon already chosen");
                return;
            }

            CurrentWeapon?.Choose(false);
            
            CurrentWeapon = nextWeapon;
            CurrentWeapon.Choose(true);

            WeaponChanged.OnNext(CurrentWeapon);
            OnWeaponChanged(nextWeapon);

            RPC_SelectViewWeapon();
        }

        public void ChooseWeapon(WeaponType weaponType)
        {
            Weapon newWeapon = Weapons.FirstOrDefault(x => x.WeaponType == weaponType);
            
            if (newWeapon != null)
                ChooseWeapon(Weapons.IndexOf(newWeapon));
            else
                AtomicLogger.Log($"Couldn't choose Weapon {weaponType}");
        }

        public void ChooseRandomWeapon()
        {
            ChooseWeapon(Random.Range(0, Weapons.Count));
        }
        

        [Rpc(Channel = RpcChannel.Reliable)]
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