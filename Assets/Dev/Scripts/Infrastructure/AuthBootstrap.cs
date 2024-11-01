using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.UI;
using Dev.Utils;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
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

            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
                AtomicLogger.Log($"UGS initialized with state: {UnityServices.State}", AtomicConstants.LogTags.Networking);

                Curtains.Instance.SetText("Authorizing");
                Curtains.Instance.ShowWithDotAnimation(0);

                await _authService.Auth();
                await _saveLoadService.Load();
            }
           
            AtomicLogger.Log($"Player ID {AuthenticationService.Instance.PlayerId}", AtomicConstants.LogTags.Networking);

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
                
                await _authService.UpdateNickname(_nicknameInput.text.Trim());
            }

            _nicknameInput.text = $"{SaveLoadService.Instance.Profile.Nickname}";
            
            Curtains.Instance.SetText("Loading data...");

            LobbyConnector lobbyConnector = Instantiate(_lobbyConnector);
            lobbyConnector.ConnectFromBootstrap();
        }
    }
}