using Dev.PlayerLogic;
using Dev.UI.PopUpsAndMenus;
using Dev.Weapons.StaticData;
using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(fileName = "GameStaticDataContainer", menuName = "StaticData/GameStaticDataContainer", order = 0)]
    public class GameStaticDataContainer : ScriptableObject
    {
        [SerializeField] private CharactersDataContainer _charactersDataContainer;
        [SerializeField] private WeaponStaticDataContainer _weaponStaticDataContainer;
        [SerializeField] private PopUpsStaticDataContainer _popUpsStaticDataContainer;

        public PopUpsStaticDataContainer PopUpsStaticDataContainer => _popUpsStaticDataContainer;
        public CharactersDataContainer CharactersDataContainer => _charactersDataContainer;
        public WeaponStaticDataContainer WeaponStaticDataContainer => _weaponStaticDataContainer;
    }
}