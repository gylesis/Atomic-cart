using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dev.UI
{
    public class LongClickReactiveButton : DefaultReactiveButton
    {
        [SerializeField] private float _holdSeconds = 2;
        [SerializeField] private Image _holdProgressImage;
        public Subject<Unit> LongClick { get; } =
            new Subject<Unit>();

        private float _timer;
        private IDisposable _longClickDisposable;
        
        private bool _longClickEnabled;
        private bool _longPressCompleted;
        
        protected override void Awake()
        {
            _clickDisposable = _button
                .OnClickAsObservable()
                .TakeUntilDestroy(this)
                .Subscribe((_ =>
                {
                    if (_longClickEnabled)
                    {
                        if(_longPressCompleted) return;
                    }

                    Clicked.OnNext(Unit.Default);
                }));

            _button.OnPointerDownAsObservable().TakeUntilDestroy(this).Subscribe((OnPointerDown));
            _button.OnPointerUpAsObservable().TakeUntilDestroy(this).Subscribe((OnPointerUp));
        }

        private void OnPointerUp(PointerEventData eventData)
        {
            OnProgressUpdate(0);
            
            if(_longClickEnabled == false) return;

            if (_longPressCompleted)
            {
                LongClick.OnNext(Unit.Default);
            }

            Observable.Timer(TimeSpan.FromTicks(1)).Subscribe((l =>
            {
                _longPressCompleted = false;
            }));
            
          
            _longClickDisposable?.Dispose();
        }

        private void OnPointerDown(PointerEventData eventData)
        {
            if(_longClickEnabled == false) return;
                
            _timer = 0;
            _longClickDisposable?.Dispose();
            
            _longClickDisposable = Observable.EveryUpdate().Subscribe((l =>
            {
                _timer += Time.deltaTime;

                if (_timer >= _holdSeconds)
                {
                    _longPressCompleted = true;

                    _longClickDisposable?.Dispose();
                    _timer = _holdSeconds;
                }
                
                OnProgressUpdate(_timer / _holdSeconds);
            }));
        }

        protected virtual void OnProgressUpdate(float progress)
        {
            _holdProgressImage.fillAmount = progress;
        }

        public void ResetProgressImage()
        {
            _holdProgressImage.fillAmount = 0;
        }

        public void SetAllowToLongClick(bool enabled)
        {
            _longClickEnabled = enabled;
        }
        
    }
}