using Dev.Infrastructure;
using Dev.Utils;
using UnityEngine;
using Zenject;

namespace Dev.Effects
{
    public class FxController : NetSingleton<FxController>
    {
        [SerializeField] private Transform _effectsParent;
        
        private FxContainer _fxContainer;
        
        [Inject]
        private void Construct(FxContainer fxContainer)
        {
            _fxContainer = fxContainer;
        }

        public Effect SpawnEffectAt(string effectName, Vector3 pos, float destroyDelay = 4)
        {
            return SpawnEffectAtInternal<Effect>(effectName, pos, destroyDelay);
        }
        
        public TEffectType SpawnEffectAt<TEffectType>(string effectName, Vector3 pos, float destroyDelay = 4) where TEffectType : Effect
        {
            return SpawnEffectAtInternal<TEffectType>(effectName, pos, destroyDelay);
        }

        protected TEffectType SpawnEffectAtInternal<TEffectType>(string effectName, Vector3 pos, float destroyDelay = 4)
            where TEffectType : Effect
        {
            var hasEffect = _fxContainer.TryGetEffectDataByName(effectName, out var effectPrefab);

            if (hasEffect)
            {   
                Effect effect = Runner.Spawn(effectPrefab, pos, default, Runner.LocalPlayer, onBeforeSpawned:
                    (runner, o) =>
                    {
                        o.transform.parent = _effectsParent;
                        o.transform.position = pos;
                    });
                
                Extensions.Delay(destroyDelay, destroyCancellationToken, () => Runner.Despawn(effect.Object));
                return effect as TEffectType;
            }

            return null;
        }
        
    }
}