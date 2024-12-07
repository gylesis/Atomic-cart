using Dev.Infrastructure.Lobby;
using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus.Lobby
{
    public class LobbyPlayMenu : PopUp
    {
        [SerializeField] private GameSessionBrowser _gameSessionBrowser;

        [SerializeField] private DefaultReactiveButton _createSessionButton;
        [SerializeField] private DefaultReactiveButton _joinSessionButton;

        protected override void Awake()
        {
            base.Awake();

            _createSessionButton.Clicked.TakeUntilDestroy(this).Subscribe(unit => OnCreateSessionButtonClicked());
            _joinSessionButton.Clicked.TakeUntilDestroy(this).Subscribe(unit => OnJoinSessionButtonClicked());

            _gameSessionBrowser.SessionCountChanged.Subscribe(OnSessionsCountChanged).AddTo(this);
        }

        private void OnCreateSessionButtonClicked()
        {
            Hide();

            PopUpService.ShowPopUp<MapSelectionMenu>((() =>
            {
                Show();
                PopUpService.HidePopUp<MapSelectionMenu>();
            }));
        }

        private void OnJoinSessionButtonClicked()
        {
            _gameSessionBrowser.JoinPickedSession();
        }

        private void OnSessionsCountChanged(int sessionsCount)
        {
            if (sessionsCount == 0)
                _joinSessionButton.Disable();
            else
                _joinSessionButton.Enable();
        }
    }
}