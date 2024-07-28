using TMPro;
using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    public class ProfileSettingsMenu : PopUp
    {
        [SerializeField] private TMP_Text _profileNameText;
        [SerializeField] private TextReactiveButton _linkAccountButton;

        protected override void Awake()
        {
            base.Awake();

            _linkAccountButton.Clicked.Subscribe((unit =>
            {
                PopUpService.ShowPopUp<LinkProfilePopUp>((() =>
                {
                    PopUpService.HidePopUp<LinkProfilePopUp>();
                }));
            })).AddTo(this);
        }

        public override void Show()
        {
            _profileNameText.text = $"{AuthService.Nickname}";
            
            base.Show();
        }
    }
}