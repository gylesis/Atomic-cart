
using Fusion;

namespace Dev.Weapons.Guns
{
    public struct WeaponData : INetworkStruct
    {
        [Networked] public int Id { get; private set; }
        [Networked] public string Name { get; private set; }

        public WeaponData(int id, string name)
        {
            Id = id;
            Name = name;
        }

    }
}