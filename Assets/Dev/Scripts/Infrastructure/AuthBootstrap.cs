using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.UI;
using Dev.UI.PopUpsAndMenus;
using Dev.Utils;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class AuthBootstrap : MonoBehaviour
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
            _nicknameInput.gameObject.SetActive(false);
            _login.gameObject.SetActive(false);
            
            await _internetChecker.Check(gameObject.GetCancellationTokenOnDestroy());
            
            Curtains.Instance.SetText("Initializing services");
            Curtains.Instance.ShowWithDotAnimation(0);

            await UnityServices.InitializeAsync();
            AtomicLogger.Log($"UGS initialized with state: {UnityServices.State}", AtomicConstants.LogTags.Networking);

            Curtains.Instance.SetText("Authorizing");
            Curtains.Instance.ShowWithDotAnimation(0);
            
            await _authService.Auth();
            
            /*DecidePopUp decidePopUp = PopUpService.Instance.ShowPopUp<DecidePopUp>();
            decidePopUp.SetTitle("Sign in anonymosly?");
            bool answer = await decidePopUp.WaitAnswer();

            PopUpService.Instance.HidePopUp(decidePopUp);
            
            if (answer)
            {
                await _authService.Auth();
            }
            else
            {
                Application.Quit();
                return;
            }*/

            if (_authService.IsNicknameNotSet)
            {
                Curtains.Instance.Hide(0);
                
                _nicknameInput.gameObject.SetActive(true);
                _login.gameObject.SetActive(true);

                while (true)
                {
                    await _login.WaitForClick();

                    if (string.IsNullOrEmpty(_nicknameInput.text) == false)
                    {
                        break;
                    }
                }
                
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