using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.Effects
{
    [CreateAssetMenu(menuName = "StaticData/FxContainer", fileName = "FxContainer", order = 0)]
    public class FxContainer : ScriptableObject
    {
        [SerializeField] private List<EffectStaticData> _effects;

        public bool TryGetEffectDataByName(string name, out Effect effect)
        {
            effect = null;
            
            EffectStaticData effectStaticData = _effects.FirstOrDefault(x => x.Name == name);

            var hasEffect = effectStaticData != null;
            
            if (hasEffect)
            {
                effect = effectStaticData.Prefab;
            }

            return hasEffect;
        }
    }
}