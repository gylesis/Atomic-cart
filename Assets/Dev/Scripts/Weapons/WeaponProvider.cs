using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Dev.Weapons.Guns;
using Dev.Weapons.StaticData;
using Fusion;
using UnityEngine;

namespace Dev.Weapons
{
    public class WeaponProvider
    {
        private WeaponStaticDataContainer _weaponStaticDataContainer;

        public WeaponProvider(GameSettings gameSettings)
        {
            _weaponStaticDataContainer = gameSettings.WeaponStaticDataContainer;
        }

        public void ProvideWeapon(NetworkRunner runner, WeaponController weaponController, WeaponType weaponType, bool withChose = false)
        {
            PlayerRef playerRef = runner.LocalPlayer;

            WeaponStaticData weaponStaticData = _weaponStaticDataContainer.GetData(weaponType);

            if (weaponController.HasWeapon(weaponStaticData.WeaponType))
            {
                if (withChose)
                {
                    weaponController.ChooseWeapon(weaponStaticData.WeaponType);
                }
                else
                {
                    AtomicLogger.Log("Weapon is already provided");
                }                
                
                return;
            }

            Weapon weaponPrefab = weaponStaticData.Prefab;
            var weaponParentTransform = weaponController.WeaponParent.transform;
            Vector3 weaponPos = weaponParentTransform.position;

            Weapon weaponInstance = runner.Spawn(weaponPrefab, weaponPos, Quaternion.Euler(0, 0, 0),
                playerRef, ((runner, o) =>
                {
                    Weapon weapon = o.GetComponent<Weapon>();
                    weapon.transform.parent = weaponParentTransform;
                    weapon.transform.localPosition = Vector3.zero;
                    weapon.transform.rotation = Quaternion.Euler(weaponParentTransform.rotation.eulerAngles);
                    weapon.transform.localScale = Vector3.one;
                }));

            weaponController.RPC_AddWeapon(weaponInstance, withChose);
        }

    }
}