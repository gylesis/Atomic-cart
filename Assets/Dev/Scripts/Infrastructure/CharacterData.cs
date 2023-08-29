using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(menuName = "StaticData/Character/CharacterData", fileName = "CharacterData", order = 0)]
    public class CharacterData : ScriptableObject   
    {
        public CharacterClass CharacterClass;
        public Player PlayerPrefab;

        [Header("Stats")]
        public CharacterStats CharacterStats;
    }
}