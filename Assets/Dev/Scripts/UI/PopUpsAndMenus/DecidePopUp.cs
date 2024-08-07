﻿using System;
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
        
        [SerializeField] private DefaultReactiveButton _yesButton;
        [SerializeField] private DefaultReactiveButton _noButton;

        [SerializeField] private Transform _centerTransform;
        
        private Action<bool> _onDecide;

        public bool Decision { get; private set; }
        
        public void Init(string title, Action<bool> onDecide)
        {
            _titleText.text = title;
            _onDecide = onDecide;
        }

        protected override void Awake()
        {
            base.Awake();

            _yesButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnDecisionDecided(true)));
            _noButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnDecisionDecided(false)));
        }

        private void OnDecisionDecided(bool isYes)
        {
            Decision = isYes;
            
            _onDecide?.Invoke(isYes);
        }

        public override void Show()
        {
            _centerTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 1;
            _backgroundImage.SetAlpha(0);
            _backgroundImage.DOFade(1, _smoothFadeInOutDuration);
            
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