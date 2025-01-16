using Dev.Weapons.StaticData;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.PlayerLogic
{
    [CreateAssetMenu(menuName = "StaticData/Character/CharacterData", fileName = "CharacterData", order = 0)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private AnimatorOverrideController _animatorController;
        [SerializeField] private Sprite _characterSprite;

        public CharacterClass CharacterClass;
        public PlayerCharacter PlayerCharacterPrefab;
        public Sprite CharacterIcon;
        public WeaponType WeaponType;

        [Header("Stats")] public CharacterStats CharacterStats;
        
        public AnimatorOverrideController AnimatorController => _animatorController;
        public Sprite CharacterSprite => _characterSprite;
    }
}