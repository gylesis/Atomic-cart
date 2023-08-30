using Fusion;

namespace Dev.Weapons
{
    public struct WeaponSetupContext : INetworkStruct
    {
        [Networked] public NetworkString<_16> Name { get; private set; }
        
        public WeaponSetupContext(string name)
        {
            Name = name;
        }
    }
}