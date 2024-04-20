using Dev.PlayerLogic;
using Dev.Weapons.StaticData;
using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(fileName = "GameStaticDataContainer", menuName = "StaticData/GameStaticDataContainer", order = 0)]
    public class GameStaticDataContainer : ScriptableObject
    {
        [SerializeField] private CharactersDataContainer _charactersDataContainer;
        [SerializeField] private WeaponStaticDataContainer _weaponStaticDataContainer;

        public CharactersDataContainer CharactersDataContainer => _charactersDataContainer;

        public WeaponStaticDataContainer WeaponStaticDataContainer => _weaponStaticDataContainer;
    }
}