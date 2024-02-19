using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class Curtains : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _loadingText;
        
        private CancellationToken _cancellationToken;

        public static Curtains Instance;

        private Color _defaultTextColor;

        private StringBuilder _loadingTextString = new StringBuilder();
        
        private void Awake()
        {
            Instance = this;
            _defaultTextColor = _text.color;

            Observable.Interval(TimeSpan.FromSeconds(0.2f)).TakeUntilDestroy(this).Subscribe((l =>
            {
                if(_canvasGroup.alpha == 0) return;
                
                _loadingTextString.Clear();

                _loadingTextString.Append("Loading");
                
                if (l % 3 == 0)
                {
                    _loadingTextString.Append("...");
                }
                else if (l % 2 == 0)
                {
                    _loadingTextString.Append("..");
                }
                else
                {
                    _loadingTextString.Append(".");
                }

                _loadingText.text = _loadingTextString.ToString();
            }));
        }

        public void SetText(string text)
        {
            _text.text = text;
        }
        
        public void SetText(string text, Color color)
        {
            _text.text = text;
            _text.color = color;
        }
        
        public async void Show(float showDuration = 1, float waitDuration = 0, Action onShow = null)
        {
            if (_text.text == String.Empty)
            {
                _text.enabled = false;
            }
            else
            {
                _text.enabled = true;
            }
            
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            await _canvasGroup.DOFade(1, showDuration).AsyncWaitForCompletion().AsUniTask();
            await UniTask.Delay(TimeSpan.FromSeconds(waitDuration), cancellationToken: _cancellationToken);
            
            onShow?.Invoke();
        }

        public void Hide(float hideDuration = 1, Action onHide = null)
        {
            _canvasGroup.DOFade(0, hideDuration).OnComplete((() =>
            {   
                _text.color = _defaultTextColor;
                _text.text = String.Empty;
                onHide?.Invoke();
            })).OnComplete((() =>
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }));
        }

        public async void HideWithDelay(float waitTime, float hideDuration = 1, Action onHide = null)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: _cancellationToken);    
            Hide(hideDuration, onHide);
        }   

        private void OnDestroy()
        {
            _cancellationToken.ThrowIfCancellationRequested();
        }
    }
}