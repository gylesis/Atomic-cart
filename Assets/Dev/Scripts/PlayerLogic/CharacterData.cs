using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.PlayerLogic
{
    [CreateAssetMenu(menuName = "StaticData/Character/CharacterData", fileName = "CharacterData", order = 0)]
    public class CharacterData : ScriptableObject
    {
        public CharacterClass CharacterClass;
        [FormerlySerializedAs("PlayerPrefab")] public PlayerCharacter _playerCharacterPrefab;

        public Sprite CharacterIcon;

        [Header("Stats")] public CharacterStats CharacterStats;
    }
}