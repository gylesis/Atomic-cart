using Cysharp.Threading.Tasks;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class PlayerStatsMenu : PopUp
    {
        [SerializeField] private TextReactiveButton _deleteAccButton;
        [SerializeField] private TMP_Text _statsText;
        
        private AuthService _authService;

        protected override void Awake()
        {
            base.Awake();
            _deleteAccButton.Clicked.Subscribe((unit => OnDeleteAccClicked()));
        }

        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }

        private async void OnDeleteAccClicked()
        {
            var decidePopUp = PopUpService.ShowPopUp<DecidePopUp>();
            
            decidePopUp.SetTitle("Are you sure to delete account?");
            decidePopUp.SetDescription("You will never be able to recover old game progress");

            bool answer = await decidePopUp.WaitAnswer();

            PopUpService.HidePopUp(decidePopUp);
            
            if (answer)
            {
                await _authService.DeleteAccountAsync();
                Application.Quit();
            }
        }

        public override void Show()
        {
            Profile profile = _authService.MyProfile;

            _statsText.text = $"Nickname: {profile.Nickname}\n" +
                              $"Kills: {profile.Kills}\n" +
                              $"Deaths: {profile.Deaths}";

            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
            
            _authService.GetMyProfileAsync(true).Forget();
        }
    }
}