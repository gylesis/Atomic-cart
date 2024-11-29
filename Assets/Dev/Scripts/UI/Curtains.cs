using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dev.Utils;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UniRx;
using UnityEngine;

namespace Dev.Infrastructure
{
    [DisallowMultipleComponent]
    public class Curtains : MonoSingleton<Curtains>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _text;

        private const float InactivityHideCooldown = 5f;
        
        private IDisposable _animationDisposable;
        private Color _defaultTextColor;
        private StringBuilder _animationStringBuilder = new StringBuilder();
        private StringBuilder _textBuilder = new StringBuilder();

        private CancellationToken _showToken = new CancellationToken();
        private CancellationToken _hideToken = new CancellationToken();
        private TweenerCore<float, float, FloatOptions> _showTween;
        private TweenerCore<float, float, FloatOptions> _hideTween;

        private bool _isActive;
        private float _lastTimeActivity;
        
        protected override void Awake()
        {
            base.Awake();
             
            DontDestroyOnLoad(gameObject);
            _defaultTextColor = _text.color;
            _lastTimeActivity = Time.realtimeSinceStartup;
        }

        public void AppendText(string text)
        {
            _lastTimeActivity = Time.realtimeSinceStartup;

            _textBuilder.Append($"\n{text}");
        }

        public void SetText(string text, bool withUpdateView = true, bool withStopDotAnimation = true)
        {
            _lastTimeActivity = Time.realtimeSinceStartup;
            
            _textBuilder.Clear();
            _textBuilder.Append(text);
            
            if (withUpdateView)
            {
                UpdateTextView();
            }

            if (withStopDotAnimation)
            {
                StopDotAnimation();
            }
        }

        public void SetTextColor(Color color)
        {
            _lastTimeActivity = Time.realtimeSinceStartup;

            _text.color = color;
        }

        public async void Show(float showDuration = 1, float waitDuration = 0, Action onShow = null)
        {
            _lastTimeActivity = Time.realtimeSinceStartup;

            _showTween.Kill(true);
            _hideTween.Kill(true);
            
            _hideToken.ThrowIfCancellationRequested();
            _showToken.ThrowIfCancellationRequested();
            
            if (_textBuilder.Length > 0)
                _text.text = _textBuilder.ToString();

            _text.enabled = _textBuilder.Length > 0;

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _showTween = _canvasGroup.DOFade(1, showDuration);
            await _showTween.AsyncWaitForCompletion();
            await UniTask.Delay(TimeSpan.FromSeconds(waitDuration), cancellationToken: _showToken);

            _isActive = true;
            
            onShow?.Invoke();
        }

        public void ShowWithDotAnimation(float fadeDuration = 1, float waitDuration = 0, Action onShow = null)
        {
            StopDotAnimation();

            string originText = _textBuilder.ToString();

            _animationDisposable = Observable.Interval(TimeSpan.FromSeconds(0.2f)).TakeUntilDestroy(this).Subscribe(
                (l =>
                {
                    _animationStringBuilder.Clear();

                    if (l % 3 == 2)
                    {
                        _animationStringBuilder.Append("...");
                    }
                    else if (l % 3 == 1)
                    {
                        _animationStringBuilder.Append("..");
                    }
                    else if (l % 3 == 0)
                    {
                        _animationStringBuilder.Append(".");
                    }
                    
                    SetText($"{originText}{_animationStringBuilder}", true, false);
                   
                }));

            Show(fadeDuration, waitDuration, onShow);
        }

        public void Hide(float hideDuration = 1, Action onHide = null)
        {
            _lastTimeActivity = Time.realtimeSinceStartup;

            _showTween.Kill(true);
            _hideTween.Kill(true);
            
            _hideToken.ThrowIfCancellationRequested();
            _showToken.ThrowIfCancellationRequested();
            
            StopDotAnimation();

            _hideTween = _canvasGroup.DOFade(0, hideDuration);
            
            _hideTween.OnComplete(() =>
            {
                ResetText();
                onHide?.Invoke();
            }).OnComplete(() =>
            {
                _isActive = false;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }).AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(_hideToken);
        }

        private void ResetText()
        {
            SetText("");
            SetTextColor(_defaultTextColor);
            UpdateTextView();
        }

        private void UpdateTextView()
        {
            _text.text = _textBuilder.ToString();
        }

        public void StopDotAnimation()
        {
            _lastTimeActivity = Time.realtimeSinceStartup;

            _animationDisposable?.Dispose();
        }

        public async void HideWithDelay(float waitTime, float hideDuration = 1, Action onHide = null)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: _showToken);
            Hide(hideDuration, onHide);
        }

        private void Update()
        {
            if(_isActive == false) return;
            
            if (Time.realtimeSinceStartup - _lastTimeActivity >= InactivityHideCooldown)
            {
                //Debug.Log($"Inactivity for curtains for {InactivityHideCooldown} secs, hiding");
                Hide(0);
            }
        }

        protected override void OnDestroy()
        {
            StopDotAnimation();
            _showToken.ThrowIfCancellationRequested();

            base.OnDestroy();
        }
    }
}