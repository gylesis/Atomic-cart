﻿using System.Linq;
using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(menuName = "StaticData/Character/CharactersDataContainer", fileName = "CharactersDataContainer", order = 0)]
    public class CharactersDataContainer : ScriptableObject
    {
        [SerializeField] private CharacterData[] _datas;

        public CharacterData GetCharacterDataByClass(CharacterClass characterClass) => _datas.First(x => x.CharacterClass == characterClass);
    }
}