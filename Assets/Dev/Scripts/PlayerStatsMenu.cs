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
                await _authService.DeleteAccount();
                Application.Quit();
            }
        }

        public override async void Show()
        {
            await SaveLoadService.Instance.Load();
            Profile profile = SaveLoadService.Instance.Profile;

            _statsText.text = $"Nickname: {profile.Nickname}\n" +
                              $"Kills: {profile.Kills}\n" +
                              $"Deaths: {profile.Deaths}";

            base.Show();
        }
    }
}