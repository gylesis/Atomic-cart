using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI
{
    public class MainMenu : PopUp
    {
        [SerializeField] private DefaultReactiveButton _showTab;
        
        private PopUpService _popUpService;

        protected override void Awake()
        {
            _showTab.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnShowTabButtonClicked()));
        }

        [Inject]
        private void Init(PopUpService popUpService)
        {
            _popUpService = popUpService;
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