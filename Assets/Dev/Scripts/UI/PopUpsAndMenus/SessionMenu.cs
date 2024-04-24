using System.Collections.Generic;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev.UI.PopUpsAndMenus
{
    public class SessionMenu : PopUp
    {
        [SerializeField] private TextReactiveButton _readyButton;
        [SerializeField] private TextReactiveButton _playButton;
        [SerializeField] private TextReactiveButton _exitButton;

        [SerializeField] private MapLobbyUI _lobbyUI;
            
        private NetworkRunner _networkRunner;

        protected override void Awake()
        {
            base.Awake();

            _networkRunner = FindObjectOfType<NetworkRunner>();

            _playButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnPlayButtonClicked()));
            _readyButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnReadyButtonClicked()));
            _exitButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnExitButtonClicked()));
            
            _lobbyUI.ReadyStatusUpdated.TakeUntilDestroy(this).Subscribe((unit => OnReadyStatusChanged()));
            
            _playButton.Disable();
        }

        private void OnExitButtonClicked()
        {
            PopUpService.TryGetPopUp<DecidePopUp>(out var decidePopUp);

            decidePopUp.Show();
            decidePopUp.Init("Are you sure want to exit?", OnDecide);

            void OnDecide(bool isYes)
            {
                PopUpService.HidePopUp<DecidePopUp>();

                if (isYes)
                {
                    // from MainSceneConnectionManager
                    
                    Curtains.Instance.Show();
                    Curtains.Instance.SetText("Returning back to menu");
            
                    LobbyConnector.IsConnected = false;
            
                    _networkRunner.Shutdown();

                    PlayerManager.PlayersOnServer.Clear();
                    PlayerManager.LoadingPlayers.Clear();

                    SceneManager.LoadScene(0);
                }
            }

        }

        private void OnReadyButtonClicked()
        {
            _lobbyUI.SetReady(_networkRunner.LocalPlayer);
        }

        private void OnReadyStatusChanged()
        {
            if (_networkRunner.IsSharedModeMasterClient)
            {
                CheckIfAllPlayersReady();
            }
        }

        private void CheckIfAllPlayersReady()
        {
            var isAllPlayersReady = _lobbyUI.IsAllPlayersReady();

            if (isAllPlayersReady)
            {
                _playButton.Enable();
            }
            else
            {
                _playButton.Disable();
            }
            
        }

        private void OnPlayButtonClicked()
        {
            _playButton.Disable();
            
            StartGame();
        }

        private async void StartGame()
        {
            var oldProperties = _networkRunner.SessionInfo.Properties;
            
            var sessionProperties = new Dictionary<string, SessionProperty>()
            {
                ["map"] = oldProperties["map"],
                ["mode"] = oldProperties["mode"],
                ["status"] = (int)SessionStatus.InGame
            };

            _networkRunner.SessionInfo.UpdateCustomProperties(sessionProperties);
            await _networkRunner.LoadScene("Main", setActiveOnLoad: true);
        }

        public override void Show()
        {
            base.Show();

            if (_networkRunner.IsSharedModeMasterClient == false)
            {
                _playButton.gameObject.SetActive(false);
            }
            
        }
    }
}