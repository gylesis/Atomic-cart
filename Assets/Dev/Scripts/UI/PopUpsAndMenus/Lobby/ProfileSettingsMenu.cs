using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI.PopUpsAndMenus
{
    public class ProfileSettingsMenu : PopUp
    {
        [SerializeField] private TMP_InputField _nickname;
        [SerializeField] private DefaultReactiveButton _changeNicknameButton;
        
        [SerializeField] private TextReactiveButton _linkAccountButton;
        private AuthService _authService;

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
            
            
            _changeNicknameButton.Clicked.Subscribe((unit => OnChangeNicknameClicked())).AddTo(this);
            _nickname.onValueChanged.AsObservable().Subscribe((OnNicknameChanged)).AddTo(this);
        }

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }

        public override void Show()
        {
            _nickname.text = $"{SaveLoadService.Instance.Profile.Nickname}";
            
            base.Show();
        }
        
        
        private async void OnNicknameChanged(string nickname)
        {
            _changeNicknameButton.IsInteractable(nickname.Length > 0);
        }

        private async void OnChangeNicknameClicked()
        {
            var updateNickname = await _authService.UpdateNickname(_nickname.text);

            if (!updateNickname)
            {
                PopUpService.ShowPopUp<NotificationPopUp>().Init("Error", $"Error updating nickname {updateNickname.ErrorMessage}", 5);
            }
            else
            {
                PopUpService.ShowPopUp<NotificationPopUp>().Init("Success", "Success changed nickname", 5);
            }

        }
    }
}