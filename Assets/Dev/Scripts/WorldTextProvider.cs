using DG.Tweening;
using UnityEngine;

namespace Dev
{
    public class WorldTextProvider : MonoBehaviour
    {
        [SerializeField] private WorldText _worldTextPrefab;

        [SerializeField] private Color _damageTextColor = Color.red;

        public void SpawnDamageText(Vector3 pos, int damage)
        {   
            pos += (Vector3)Random.insideUnitCircle * 4;

            Quaternion rotation = Quaternion.identity;
            Vector3 eulerAngles = Vector3.zero;
            eulerAngles.x = 0;
            eulerAngles.y = 0;
            eulerAngles.z = Random.Range(-20f, 20f);

            rotation.eulerAngles = eulerAngles;
            
            WorldText worldText = Instantiate(_worldTextPrefab, pos, rotation);

            string text = $"-{damage}";
            
            worldText.Init(text, _damageTextColor);

            Sequence sequence = DOTween.Sequence();

            float scale = Random.Range(1.2f, 1.6f);
            
            sequence
                .Append(worldText.transform.DOScale(scale, 0.5f).SetEase(Ease.OutBounce))
                .AppendInterval(0.5f)
                .Append(worldText.transform.DOScale(0,0.4f))
                .AppendCallback((() =>
                {
                    Destroy(worldText);
                }));

            sequence.Play();
        }
        
        
    }
}