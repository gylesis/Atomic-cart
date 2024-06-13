using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.PlayerLogic
{
    [CreateAssetMenu(menuName = "StaticData/Character/CharacterData", fileName = "CharacterData", order = 0)]
    public class CharacterData : ScriptableObject
    {
        public CharacterClass CharacterClass;
        [FormerlySerializedAs("_playerCharacterPrefab")] [FormerlySerializedAs("PlayerPrefab")] public PlayerCharacter PlayerCharacterPrefab;

        public Sprite CharacterIcon;

        [Header("Stats")] public CharacterStats CharacterStats;
    }
}