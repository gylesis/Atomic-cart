using System;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Dev.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopUp : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected DefaultReactiveButton _procceedButton;

        [SerializeField] private float _smoothFadeInOutDuration = 1f;

        public Subject<bool> OnHide { get; } = new Subject<bool>();

        private IDisposable _disposable;

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _procceedButton = GetComponentInChildren<DefaultReactiveButton>();
        }

        public void OnSucceedButtonClicked(Action action)
        {
            _disposable?.Dispose();
            _disposable = _procceedButton.Clicked.Subscribe((unit => action.Invoke()));
        }

        protected void SimulateSucceedButtonClick()
        {
            _procceedButton.Clicked.OnNext(Unit.Default);
        }

        private void Awake()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public virtual void Show()
        {
            EnableSmooth();
        }

        public virtual void Hide()
        {
            DisableSmooth();
        }

        protected void EnableCanvasGroup()
        {
            OnHide.OnNext(false);
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        protected void DisableCanvasGroup()
        {
            OnHide.OnNext(true);
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        protected void EnableSmooth()
        {
            EnableCanvasGroup();
            _canvasGroup.DOFade(1, _smoothFadeInOutDuration);
        }

        protected void DisableSmooth()
        {
            DisableCanvasGroup();
            _canvasGroup.DOFade(0, _smoothFadeInOutDuration);
        }

        protected virtual void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}