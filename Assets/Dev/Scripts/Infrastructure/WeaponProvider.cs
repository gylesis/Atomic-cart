using Dev.PlayerLogic;
using Dev.Weapons;
using Dev.Weapons.Guns;
using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class WeaponProvider
    {
        private WeaponStaticDataContainer _weaponStaticDataContainer;
        private NetworkRunner _runner;

        public WeaponProvider(WeaponStaticDataContainer weaponStaticDataContainer, NetworkRunner runner)
        {
            _runner = runner;
            _weaponStaticDataContainer = weaponStaticDataContainer;
        }

        public void ProvideWeaponToPlayer<TWeaponType>(PlayerRef playerRef, bool withChose = false)
            where TWeaponType : Weapon
        {
            Player player = _runner.GetPlayerObject(playerRef).GetComponent<Player>();

            WeaponController playerWeaponController = player.WeaponController;

            WeaponStaticData weaponStaticData = _weaponStaticDataContainer.GetData<TWeaponType>();

            if (playerWeaponController.HasWeapon(weaponStaticData.WeaponName))
            {
                if (withChose)
                {
                    playerWeaponController.RPC_ChooseWeapon(weaponStaticData.WeaponName);
                }

                Debug.Log($"This weapon is already chosen");
                return;
            }

            Weapon weaponPrefab = weaponStaticData.Prefab;

            Vector3 weaponPos = player.WeaponController.WeaponParent.transform.position;

            Weapon weaponInstance = _runner.Spawn(weaponPrefab, weaponPos, Quaternion.Euler(0, 0, 0),
                playerRef, ((runner, o) =>
                {
                    Weapon weapon = o.GetComponent<Weapon>();
                    var weaponData = new WeaponData(-1, weaponStaticData.WeaponName);
                    weapon.Init(weaponData);
                    weapon.RPC_SetPos(weaponPos);
                    weapon.RPC_SetRotation(player.WeaponController.WeaponParent.rotation.eulerAngles);
                }));

            playerWeaponController.RPC_AddWeapon(weaponInstance, withChose);
        }

        public void ProvideWeaponToPlayer(PlayerRef playerRef, string weaponName, bool withChose = false)
        {
            Player player = _runner.GetPlayerObject(playerRef).GetComponent<Player>();

            WeaponController playerWeaponController = player.WeaponController;

            WeaponStaticData weaponStaticData = _weaponStaticDataContainer.GetData(weaponName);

            if (playerWeaponController.HasWeapon(weaponStaticData.WeaponName))
            {
                if (withChose)
                {
                    playerWeaponController.RPC_ChooseWeapon(weaponStaticData.WeaponName);
                }

                Debug.Log($"This weapon is already chosen");
                return;
            }

            Weapon weaponPrefab = weaponStaticData.Prefab;

            Vector3 weaponPos = player.WeaponController.WeaponParent.transform.position;

            Weapon weaponInstance = _runner.Spawn(weaponPrefab, weaponPos, Quaternion.Euler(0, 0, 0),
                playerRef, ((runner, o) =>
                {
                    Weapon weapon = o.GetComponent<Weapon>();
                    var weaponData = new WeaponData(-1, weaponStaticData.WeaponName);
                    weapon.Init(weaponData);
                    weapon.RPC_SetPos(weaponPos);
                    weapon.RPC_SetRotation(player.WeaponController.WeaponParent.rotation.eulerAngles);
                }));

            playerWeaponController.RPC_AddWeapon(weaponInstance, withChose);
        }
    }
}