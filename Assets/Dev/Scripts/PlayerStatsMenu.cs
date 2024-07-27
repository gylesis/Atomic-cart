using Dev.UI.PopUpsAndMenus;
using TMPro;
using UnityEngine;

namespace Dev
{
    public class PlayerStatsMenu : PopUp
    {
        [SerializeField] private TMP_Text _statsText;

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