using System;
using Dev.Weapons.Guns;

namespace Dev.Weapons
{
    [Serializable]
    public class WeaponStaticData
    {
        public string WeaponName = "none";
        public Weapon Prefab;
        public Type WeaponType => Prefab.GetType();
    }
}