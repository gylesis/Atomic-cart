using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.Weapons.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/WeaponStaticDataContainer", fileName = "WeaponStaticDataContainer", order = 0)]
    public class WeaponStaticDataContainer : ScriptableObject
    {
        [SerializeField] private List<WeaponStaticData> _weaponStaticDatas;
        
        public WeaponStaticData GetData(WeaponType weaponType) 
        {
            return _weaponStaticDatas.First(x => x.WeaponType == weaponType);
        }
        
        public TSystemType GetData<TSystemType>() where TSystemType : WeaponStaticData
        {       
            return _weaponStaticDatas.First(x => x.GetType() == typeof(TSystemType)) as TSystemType;
        }
    }
}