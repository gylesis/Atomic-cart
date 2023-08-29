using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Weapons.Guns;
using UnityEngine;

namespace Dev.Weapons
{
    [CreateAssetMenu(menuName = "StaticData/WeaponStaticDataContainer", fileName = "WeaponStaticDataContainer", order = 0)]
    public class WeaponStaticDataContainer : ScriptableObject
    {
        [SerializeField] private List<WeaponStaticData> _weaponStaticDatas;

        public WeaponStaticData GetData<TWeaponType>() where TWeaponType : Weapon
        {
            return _weaponStaticDatas.First(x => x.WeaponType == typeof(TWeaponType));
        }
        
        public WeaponStaticData GetData(string weaponName) 
        {
            return _weaponStaticDatas.First(x => x.WeaponName == weaponName);
        }
        
        private void OnValidate()
        {
            foreach (WeaponStaticData data in _weaponStaticDatas)
            {
                if (data.WeaponName == String.Empty)
                {
                    if (data.Prefab != null)
                    {
                        data.WeaponName = data.WeaponType.Name;
                    }
                }
            }
        }
    }
}