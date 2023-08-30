using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI
{
    public class MainMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _showTab;
        
        protected override void Awake()
        {
            _showTab.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnShowTabButtonClicked()));
        }
        
        private void OnShowTabButtonClicked()
        {
            var tryGetPopUp = PopUpService.TryGetPopUp<PlayersScoreMenu>(out var playersScoreMenu);

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