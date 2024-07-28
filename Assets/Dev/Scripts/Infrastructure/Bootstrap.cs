using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.UI;
using Dev.Utils;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nicknameInput;
        [SerializeField] private DefaultReactiveButton _login;
        [SerializeField] private LobbyConnector _lobbyConnector;
        
        private SaveLoadService _saveLoadService;
        private AuthService _authService;
        private InternetChecker _internetChecker;

        [Inject]
        private void Construct(AuthService authService, SaveLoadService saveLoadService, InternetChecker internetChecker)
        {
            _internetChecker = internetChecker;
            _authService = authService;
            _saveLoadService = saveLoadService;
        }

        private async void Start()
        {
            _nicknameInput.gameObject.SetActive(true);
            _login.gameObject.SetActive(false);

            await _internetChecker.Check(gameObject.GetCancellationTokenOnDestroy());
            
            Curtains.Instance.SetText("Initializing services");
            Curtains.Instance.ShowWithDotAnimation(0);
            
            await UnityServices.InitializeAsync();
            AtomicLogger.Log($"UGS initialized with state: {UnityServices.State}", AtomicConstants.LogTags.Networking);

            AtomicLogger.Log($"SessionTokenExists: {AuthenticationService.Instance.SessionTokenExists}");
            
            Curtains.Instance.SetText("Authorizing");
            Curtains.Instance.ShowWithDotAnimation(0);

            bool auth = await _authService.Auth().AsUniTask().AttachExternalCancellation(gameObject.GetCancellationTokenOnDestroy());

            if (auth == false)
            {
                return;
            }

            if (_authService.IsNicknameNotSet)
            {
                _login.gameObject.SetActive(true);
                
                await _login.WaitForClick();

                _nicknameInput.interactable = false;
                _login.gameObject.SetActive(false);
                
                await _authService.UpdateNickname(_nicknameInput.text);
            }
            else
            {
                await _authService.UpdateNickname(default);
            }

            _nicknameInput.text = $"{AuthService.Nickname}";
            
            Curtains.Instance.SetText("Loading data...");
            
            await _saveLoadService.Load().AsUniTask().AttachExternalCancellation(gameObject.GetCancellationTokenOnDestroy());

            LobbyConnector lobbyConnector = Instantiate(_lobbyConnector);
            lobbyConnector.ConnectFromBootstrap();
        }
    }
}