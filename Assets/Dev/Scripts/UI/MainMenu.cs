using UniRx;
using UnityEngine;

namespace Dev.UI
{
    public class MainMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _showTab;
        
        private PopUpService _popUpService;

        protected override void Awake()
        {
            _showTab.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnShowTabButtonClicked()));
            _popUpService = FindObjectOfType<PopUpService>();
        }

        private void OnShowTabButtonClicked()
        {
            var tryGetPopUp = _popUpService.TryGetPopUp<PlayersScoreMenu>(out var playersScoreMenu);

            if (tryGetPopUp)
            {
                Hide();
                
                playersScoreMenu.Show();
                
                playersScoreMenu.OnSucceedButtonClicked((() =>
                {
                    playersScoreMenu.Hide();
                    Show();
                }));
            }
            
        }
        
    }
}