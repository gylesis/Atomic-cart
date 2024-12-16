using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Utils;
using UnityEngine;

namespace Dev.Effects
{
    public class FxController : NetworkContext
    {
        [SerializeField] private FxContainer _fxContainer;
        [SerializeField] private Transform _effectsParent;
        
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

        public TEffectType SpawnEffectAt<TEffectType>(string effectName, Vector3 pos, float destroyDelay = 4) where TEffectType : Effect
        {
            var hasEffect = _fxContainer.TryGetEffectDataByName(effectName, out var effectPrefab);

            if (hasEffect)
            {   
                Effect effect = Runner.Spawn(effectPrefab, pos, default, Runner.LocalPlayer);
                effect.transform.parent = _effectsParent;
                Extensions.Delay(destroyDelay, destroyCancellationToken, (() => Runner.Despawn(effect.Object)));
                return effect as TEffectType;
            }

            return null;
        }
        
        public Effect SpawnEffectAt(string effectName, Vector3 pos, float destroyDelay = 4) => SpawnEffectAt<Effect>(effectName, pos, destroyDelay);


        /*[Rpc(Channel = RpcChannel.Reliable)]
        private void RPC_SpawnEffect(string effectName, Vector3 pos, float destroyDelay)
        {   
            var hasEffect = _fxContainer.TryGetEffectDataByName(effectName, out var effectPrefab);

            if (hasEffect)
            {   
                Effect effect = Runner.Spawn(effectPrefab, pos, default, Runner.LocalPlayer);
                Extensions.Delay(destroyDelay, destroyCancellationToken, (() => Runner.Despawn(effect.Object)));
                return effect as TEffectType;
            }
        }*/

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