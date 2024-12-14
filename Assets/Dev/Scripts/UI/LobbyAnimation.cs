using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Dev.UI
{
    public class LobbyAnimation : MonoSingleton<LobbyAnimation>
    {
        [SerializeField] private Image _icon;
        
        [SerializeField] private List<Sprite> _imageList;
        [SerializeField] private float _fadeDuration = 1;
        [SerializeField] private float _waitCooldown = 3f;
        
        private int _currentIndex;
        private int _prevIndex = -1;

        private bool _isPlaying;
        
        public async void Play()
        {
            _prevIndex = -1;
            _isPlaying = true;
            
            while (_isPlaying)
            {
                _currentIndex = Random.Range(0, _imageList.Count - 1);

                Sprite currentSprite = _imageList[_currentIndex];

                _icon.sprite = currentSprite;
                _icon.color = new Color(_icon.color.r, _icon.color.g, _icon.color.b, 0);
                
                await _icon.DOFade(1, _fadeDuration).AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(gameObject.GetCancellationTokenOnDestroy());
                await UniTask.Delay(TimeSpan.FromSeconds(_waitCooldown), cancellationToken: gameObject.GetCancellationTokenOnDestroy());
                await _icon.DOFade(0, _fadeDuration).AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(gameObject.GetCancellationTokenOnDestroy());
            }
        }

        public void Stop()
        {
            _isPlaying = false;
        }
        
        
    }
}