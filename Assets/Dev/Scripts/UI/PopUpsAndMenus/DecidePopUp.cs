using System;
using Cysharp.Threading.Tasks;
using Dev.Utils;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.UI.PopUpsAndMenus
{
    public class DecidePopUp : PopUp
    {
        [SerializeField] private Image _backgroundImage;

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Transform _descriptionTransform;

        [SerializeField] private DefaultReactiveButton _yesButton;
        [SerializeField] private DefaultReactiveButton _noButton;

        [SerializeField] private Transform _centerTransform;

        private Action<bool> _onDecide;
        private UniTaskCompletionSource<bool> _uniTaskCompletionSource;

        protected override void Awake()
        {
            base.Awake();

            _uniTaskCompletionSource = new UniTaskCompletionSource<bool>();

            _yesButton.Clicked.Subscribe((unit => OnDecisionDecided(true))).AddTo(this);
            _noButton.Clicked.Subscribe((unit => OnDecisionDecided(false))).AddTo(this);
        }

        public void SetTitle(string title)
        {
            _titleText.text = title;
        }

        public void SetDescription(string description)
        {
            _descriptionText.text = description;
        }

        public void AddCallbackOnDecide(Action<bool> onDecide)
        {
            _onDecide = onDecide;
        }

        private void OnDecisionDecided(bool isYes)
        {
            _onDecide?.Invoke(isYes);
            _uniTaskCompletionSource?.TrySetResult(isYes);

            _descriptionText.text = String.Empty;
            _titleText.text = String.Empty;
        }

        public UniTask<bool> WaitAnswer()
        {
            return _uniTaskCompletionSource.Task;
        }

        public override void Show()
        {
            _centerTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 1;
            _backgroundImage.SetAlpha(0);
            _backgroundImage.DOFade(1, _smoothFadeInOutDuration);

            _descriptionTransform.gameObject.SetActive(_descriptionText.text != String.Empty);

            EnableCanvasGroup();

            _centerTransform.DOScale(1, _smoothFadeInOutDuration).SetEase(Ease.OutBounce);
        }

        public override void Hide()
        {
            _backgroundImage.DOFade(0, _smoothFadeInOutDuration);

            DisableCanvasGroup();

            _centerTransform.DOScale(0, _smoothFadeInOutDuration).SetEase(Ease.OutBounce);
        }
    }
}