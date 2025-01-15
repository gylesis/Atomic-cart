using UnityEngine;

namespace Dev
{
    public class DangerZoneView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        public void SetRadius(float dangerRadius)
        {
            transform.localScale = Vector3.one * (dangerRadius * 2);                         
        }
    }
}