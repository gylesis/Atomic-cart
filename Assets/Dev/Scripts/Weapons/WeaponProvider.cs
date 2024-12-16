using Dev.PlayerLogic;
using Dev.Weapons.Guns;
using Dev.Weapons.StaticData;
using Fusion;
using UnityEngine;

namespace Dev.Weapons
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

        public void ProvideWeaponToPlayer(PlayerRef playerRef, WeaponType weaponType, bool withChose = false)
        {
            PlayerCharacter playerCharacter = _runner.GetPlayerObject(playerRef).GetComponent<PlayerCharacter>();

            WeaponController playerWeaponController = playerCharacter.WeaponController;

            WeaponStaticData weaponStaticData = _weaponStaticDataContainer.GetData(weaponType);

            if (playerWeaponController.HasWeapon(weaponStaticData.WeaponType))
            {
                if (withChose)
                {
                    playerWeaponController.ChooseWeapon(weaponStaticData.WeaponType);
                }

                Debug.Log($"This weapon is already chosen");
                return;
            }

            Weapon weaponPrefab = weaponStaticData.Prefab;

            Vector3 weaponPos = playerCharacter.WeaponController.WeaponParent.transform.position;

            Weapon weaponInstance = _runner.Spawn(weaponPrefab, weaponPos, Quaternion.Euler(0, 0, 0),
                playerRef, ((runner, o) =>
                {
                    Weapon weapon = o.GetComponent<Weapon>();
                    weapon.RPC_SetPos(weaponPos);
                    weapon.RPC_SetRotation(playerCharacter.WeaponController.WeaponParent.rotation.eulerAngles);
                }));

            playerWeaponController.RPC_AddWeapon(weaponInstance, withChose);
        }

    }
}