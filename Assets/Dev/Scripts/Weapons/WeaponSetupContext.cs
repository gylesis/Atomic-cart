using Dev.Weapons.StaticData;
using Fusion;

namespace Dev.Weapons
{
    public struct WeaponSetupContext : INetworkStruct
    {
        [Networked] public WeaponType WeaponType { get; private set; }
        
        public WeaponSetupContext(WeaponType weaponType)
        {
            WeaponType = weaponType;
        }
    }
}