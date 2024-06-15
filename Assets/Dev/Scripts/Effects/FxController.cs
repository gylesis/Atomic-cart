using System;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Effects
{
    public class FxController : NetworkContext
    {
        [SerializeField] private FxContainer _fxContainer;

        public static FxController Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public TEffectType SpawnEffectAt<TEffectType>(string effectName, Vector3 pos, Quaternion rotation = default, float destroyDelay = 4) where TEffectType : Effect
        {
            var hasEffect = _fxContainer.TryGetEffectDataByName(effectName, out var effectPrefab);

            if (hasEffect)
            {   
                Effect effect = Runner.Spawn(effectPrefab, pos, rotation, Runner.LocalPlayer);

                Observable.Timer(TimeSpan.FromSeconds(destroyDelay)).TakeUntilDestroy(this).Subscribe((l => { Runner.Despawn(effect.Object); }));
                
                return effect as TEffectType;
                
            }

            return null;
        }   

        /*public Effect SpawnEffectAt(string effectName, Transform parent, Quaternion rotation = default)
        {
            var hasEffect = _fxContainer.TryGetEffectDataByName(effectName, out var effect);

            if (hasEffect)
            {
                Effect effectInstance = Instantiate(effect, parent.position, rotation, parent);

                Destroy(effectInstance.gameObject, 5f);
                
                return effectInstance;
            }

            return null;
        }*/
    }
}