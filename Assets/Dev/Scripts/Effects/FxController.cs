using System;
using Dev.Infrastructure;
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


        public void SpawnEffectAt(string effectName, Vector3 pos, Quaternion rotation = default)
        {
            var hasEffect = _fxContainer.TryGetEffectDataByName(effectName, out var effectPrefab);

            if (hasEffect)
            {
                Effect effect = Runner.Spawn(effectPrefab, pos, rotation, Object.InputAuthority);

                effect.RPC_SetPos(pos);

                Observable.Timer(TimeSpan.FromSeconds(4)).Subscribe((l => { Runner.Despawn(effect.Object); }));
            }
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