using System.Linq;
using UnityEngine;

namespace Dev.PlayerLogic
{
    [CreateAssetMenu(menuName = "StaticData/Character/CharactersDataContainer", fileName = "CharactersDataContainer")]
    public class CharactersDataContainer : ScriptableObject
    {
        [SerializeField] private CharacterData[] _datas;

        public CharacterData GetCharacterDataByClass(CharacterClass characterClass) =>
            _datas.First(x => x.CharacterClass == characterClass);

        public CharacterData[] Datas => _datas;
    }
}