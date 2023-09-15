using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.UI
{
    public class ExitPopUp : PopUp
    {
        [SerializeField] private DefaultReactiveButton _exitButton;
        
        protected override void Awake()
        {
            base.Awake();

            _exitButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitButtonClicked()));
        }

        private void OnExitButtonClicked()
        {
            PopUpService.TryGetPopUp<DecidePopUp>(out var decidePopUp);

            decidePopUp.Show();
            decidePopUp.Init("Are you sure want to exit?", OnDecide);

            void OnDecide(bool isYes)
            {
                decidePopUp.Hide();

                if (isYes)
                {
                    ConnectionManager.Instance.Disconnect();
                }
                else
                {
                    
                }
            }

        }
    }
}