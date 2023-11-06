using System;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopUp : MonoBehaviour
    {
        [Header("Sets if pop up or menus is root UI screen")]
        [SerializeField] private bool _isRoot;
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected DefaultReactiveButton _procceedButton;

        [SerializeField] protected float _smoothFadeInOutDuration = 1f;

        public bool IsRoot => _isRoot;

        public Subject<bool> ShowAndHide { get; } = new Subject<bool>();

        private IDisposable _disposable;
        protected PopUpService PopUpService;

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _procceedButton = GetComponentInChildren<DefaultReactiveButton>();
        }

        public void InitPopUpService(PopUpService popUpService)
        {
            PopUpService = popUpService;
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

        protected virtual void Awake()
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
            ShowAndHide.OnNext(false);
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        protected void DisableCanvasGroup()
        {
            ShowAndHide.OnNext(true);
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

        protected void EnableScaled()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.transform.localScale = Vector3.zero;
            EnableCanvasGroup();
            _canvasGroup.transform.DOScale(1, _smoothFadeInOutDuration).SetEase(Ease.OutBounce);
        }

        protected void DisableScaled()
        {
            _canvasGroup.transform.localScale = Vector3.one;
            DisableCanvasGroup();
            _canvasGroup.transform.DOScale(0, _smoothFadeInOutDuration).SetEase(Ease.OutBounce).OnComplete((() =>
            {
                _canvasGroup.alpha = 0;
            }));
        }

        protected virtual void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}