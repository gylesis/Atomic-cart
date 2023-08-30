using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(menuName = "StaticData/Character/CharacterData", fileName = "CharacterData", order = 0)]
    public class CharacterData : ScriptableObject   
    {
        public CharacterClass CharacterClass;
        public Player PlayerPrefab;

        public Sprite CharacterIcon;
        
        [Header("Stats")]
        public CharacterStats CharacterStats;
    }
}