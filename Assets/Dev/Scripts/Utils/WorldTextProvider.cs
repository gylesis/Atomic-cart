using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Dev.Utils
{
    public class WorldTextProvider : MonoBehaviour
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private WorldText _worldTextPrefab;
        
        private ObjectPool<WorldText> _worldTextPool;

        private void Awake()
        {
            _worldTextPool = new ObjectPool<WorldText>(CreateFunc, ActionOnGet, ActionOnRelease, null, true, 25);
        }

        private void ActionOnRelease(WorldText obj)
        {
            obj.gameObject.SetActive(false);
            
            obj.transform.localScale = Vector3.one;
            obj.transform.rotation = Quaternion.identity;
        }

        private void ActionOnGet(WorldText obj)
        {
            obj.gameObject.SetActive(true);
        }

        private WorldText CreateFunc()
        {
            WorldText worldText = Instantiate(_worldTextPrefab, _parent);

            return worldText;
        }

        public void SpawnDamageText(Vector3 pos, int damage, Color color)
        {   
            pos += (Vector3)Random.insideUnitCircle * 4;

            Quaternion rotation = Quaternion.identity;
            Vector3 eulerAngles = Vector3.zero;
            eulerAngles.x = 0;
            eulerAngles.y = 0;
            eulerAngles.z = Random.Range(-20f, 20f);

            rotation.eulerAngles = eulerAngles;

            WorldText worldText = _worldTextPool.Get();
            worldText.transform.position = pos;
            worldText.transform.rotation = rotation;

            string text = $"-{damage}";
            
            worldText.Setup(text, color);

            Sequence sequence = DOTween.Sequence();

            float scale = Random.Range(1.2f, 1.6f);
            
            sequence
                .Append(worldText.transform.DOScale(scale, 0.5f).SetEase(Ease.OutBounce))
                .AppendInterval(0.5f)
                .Append(worldText.transform.DOScale(0,0.4f))
                .AppendCallback((() =>
                {
                    _worldTextPool.Release(worldText);
                }));

            sequence.Play();
        }
    }
}