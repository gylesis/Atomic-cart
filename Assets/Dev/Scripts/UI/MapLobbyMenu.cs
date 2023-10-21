using Dev.Infrastructure;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.UI
{
    public class MapLobbyMenu : PopUp
    {
        [SerializeField] private TextReactiveButton _readyButton;
        [SerializeField] private TextReactiveButton _playButton;

        [SerializeField] private MapLobbyUI _lobbyUI;
        
        private SceneLoader _sceneLoader;
        private NetworkRunner _networkRunner;

        protected override void Awake()
        {
            base.Awake();

            _playButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnPlayButtonClicked()));
            _readyButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnReadyButtonClicked()));
            _lobbyUI.ReadyStatusUpdated.TakeUntilDestroy(this).Subscribe((unit => OnReadyStatusChanged()));
            
            _playButton.Disable();
        }

        private void OnReadyStatusChanged()
        {
            if (_networkRunner.IsSharedModeMasterClient)
            {
                CheckIfAllPlayersReady();
            }
        }

        [Inject]
        private void Init(SceneLoader sceneLoader, NetworkRunner networkRunner)
        {
            _networkRunner = networkRunner;
            _sceneLoader = sceneLoader;
        }
        
        private void OnReadyButtonClicked()
        {
            _lobbyUI.RPC_SetReady(_networkRunner.LocalPlayer);

            var isPlayerReady = _lobbyUI.IsPlayerReady(_networkRunner.LocalPlayer);

           
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
            _sceneLoader.LoadScene("Main");
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