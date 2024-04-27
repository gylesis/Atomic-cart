using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dev.UI.PopUpsAndMenus
{
    public class LongClickReactiveButton : DefaultReactiveButton
    {
        [SerializeField] private float _holdSeconds = 2;
        [SerializeField] private Image _holdProgressImage;
        public Subject<Unit> LongClick { get; } =
            new Subject<Unit>();

        private float _timer;
        private IDisposable _disposable;
        private bool _longClickEnabled;

        protected override void Awake()
        {
            base.Awake();

            _button.OnPointerDownAsObservable().TakeUntilDestroy(this).Subscribe((OnPointerDown));
            _button.OnPointerUpAsObservable().TakeUntilDestroy(this).Subscribe((OnPointerUp));
        }

        private void OnPointerUp(PointerEventData eventData)
        {
            OnProgressUpdate(0);
            
            if(_longClickEnabled == false) return;
            
            _disposable?.Dispose();
        }

        private void OnPointerDown(PointerEventData eventData)
        {
            if(_longClickEnabled == false) return;
                
            _timer = 0;
            _disposable?.Dispose();
            
            _disposable = Observable.EveryUpdate().Subscribe((l =>
            {
                _timer += Time.deltaTime;

                if (_timer >= _holdSeconds)
                {
                    LongClick.OnNext(Unit.Default);
                    _disposable?.Dispose();
                    _timer = _holdSeconds;
                }

                OnProgressUpdate(_timer);
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