using UnityEngine;

namespace Dev.Levels.Env
{
    public class Cloud : MonoBehaviour
    {
        [SerializeField] private Transform _view;
        [SerializeField] private SpriteRenderer _cloudSprite;
        
        private float _size;
        private float _moveSpeed;

        public void Setup(float size, float moveSpeed, Sprite sprite)
        {
            _moveSpeed = moveSpeed;
            _size = size;
            
            transform.localScale = Vector3.one * size;
            UpdateSprite(sprite);
        }

        public void UpdateSprite(Sprite sprite)
        {
            _cloudSprite.sprite = sprite;
        }

        public void Move(Vector3 moveDirection)
        {
            transform.position += moveDirection * (_moveSpeed * Time.deltaTime);
        }
        
    }
}