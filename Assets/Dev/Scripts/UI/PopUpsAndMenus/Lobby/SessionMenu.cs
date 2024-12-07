using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Infrastructure.Lobby;
using Dev.Infrastructure.Networking;
using Dev.UI.PopUpsAndMenus.Other;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.UI.PopUpsAndMenus.Lobby
{
    public class SessionMenu : PopUp
    {
        [SerializeField] private ReadyUI[] _readyUis;
        
        [SerializeField] private TextReactiveButton _readyButton;
        [SerializeField] private TextReactiveButton _playButton;
        [SerializeField] private TextReactiveButton _exitButton;

        private NetworkRunner _networkRunner;
        private SceneLoader _sceneLoader;
        private SessionController _sessionController;

        [Inject]
        private void Construct(SceneLoader sceneLoader, SessionController sessionController)
        {
            _sessionController = sessionController;
            _sceneLoader = sceneLoader;
        }
        
        protected override async void Awake()
        {
            base.Awake();

            LobbyConnector lobbyConnector = await LobbyConnector.WaitForInitialization();
            
            _networkRunner = lobbyConnector.NetworkRunner;

            _playButton.Clicked.Subscribe(unit => OnPlayButtonClicked()).AddTo(GlobalDisposable.SceneScopeToken);
            _readyButton.Clicked.Subscribe(unit => OnReadyButtonClicked()).AddTo(GlobalDisposable.SceneScopeToken);
            _exitButton.Clicked.Subscribe(unit => OnExitButtonClicked()).AddTo(GlobalDisposable.SceneScopeToken);
            
            _sessionController.ReadyStatusUpdated.Subscribe(unit => OnReadyStatusChanged()).AddTo(GlobalDisposable.SceneScopeToken);
            _sessionController.PlayerJoinedSession.Subscribe(OnPlayerJoinedSession).AddTo(GlobalDisposable.SceneScopeToken);
            _sessionController.PlayerLeftSession.Subscribe(OnPlayerLeftSession).AddTo(GlobalDisposable.SceneScopeToken);
            
            _playButton.Disable();
        }

        public override void Show()
        {
            _playButton.gameObject.SetActive(_networkRunner.IsSharedModeMasterClient);
            base.Show();
        }

        private async void StartGame()
        {
            var oldProperties = _networkRunner.SessionInfo.Properties;
            
            var sessionProperties = new Dictionary<string, SessionProperty>()
            {
                ["map"] = oldProperties["map"],
                ["mode"] = oldProperties["mode"],
                ["status"] = (int)SessionStatus.InGame,
            };
            
            _networkRunner.SessionInfo.UpdateCustomProperties(sessionProperties);
            await _networkRunner.LoadScene("Main", setActiveOnLoad: true);
        }

        private void OnReadyStatusChanged()
        {
            UpdateState();

            if (_networkRunner.IsSharedModeMasterClient) 
                CheckIfAllPlayersReady();
        }

        private void OnPlayerJoinedSession(PlayerRef playerRef)
        {
            ReadyUI readyUI = _readyUis.First(x => x.PlayerRef == PlayerRef.None);
            
            Extensions.Delay(0.5f, destroyCancellationToken, () =>
            {
                readyUI.RPC_AssignPlayer(playerRef);
            });
        }

        private void OnPlayerLeftSession(PlayerRef playerRef)
        {
            ReadyUI readyUI = _readyUis.First(x => x.PlayerRef == playerRef);
            AtomicLogger.Log($"Player {playerRef} left lobby");
            readyUI.RPC_RemovePlayerAssigment();
        }

        private void UpdateState()
        {
            foreach (var valuePair in _sessionController.SessionPlayerReadyStatuses)
            {
                bool isReady = valuePair.Value;
                PlayerRef playerRef = valuePair.Key;

                ReadyUI readyUI = _readyUis.FirstOrDefault(x => x.PlayerRef == playerRef);

                if(readyUI == null)
                    continue;
                
                readyUI.RPC_SetReadyView(isReady);
                readyUI.UpdateNickname();
            }
        }

        private void CheckIfAllPlayersReady()
        {
            var isAllPlayersReady = _sessionController.IsAllPlayersReady();

            if (isAllPlayersReady)
                _playButton.Enable();
            else
                _playButton.Disable();
        }

        private void OnReadyButtonClicked()
        {
            PlayerRef playerRef = _networkRunner.LocalPlayer;

            bool wasReady = _sessionController.IsPlayerReady(playerRef);
            _sessionController.SetReady(playerRef);

            _readyUis.First(x => x.PlayerRef == playerRef).RPC_SetReadyView(!wasReady);
        }

        private void OnPlayButtonClicked()
        {
            _playButton.Disable();
            StartGame();
        }

        private async void OnExitButtonClicked()
        {
            var decidePopUp = PopUpService.ShowPopUp<DecidePopUp>();

            decidePopUp.Show();
            decidePopUp.SetTitle("Are you sure want to exit?");

            bool answer = await decidePopUp.WaitAnswer();

            PopUpService.HidePopUp<DecidePopUp>();

            if (answer)
            {
                // from MainSceneConnectionManager
                    
                Curtains.Instance.Show();
                Curtains.Instance.SetText("Returning back to menu");
            
                LobbyConnector.Instance.IsConnected = false;
            
                await _networkRunner.Shutdown();
                
                PlayerManager.PlayersOnServer.Clear();
                PlayerManager.LoadingPlayers.Clear();
                
                _sceneLoader.LoadSceneLocal(0, LoadSceneMode.Single).Forget();
            }

        }
    }
}