using Dev.Infrastructure;
using Dev.UI;
using Dev.Utils;
using TMPro;
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

            await _internetChecker.Check();
            
            await _authService.Auth();
            
            if (_authService.IsNicknameNotSet())
            {
                _login.gameObject.SetActive(true);
                
                await _login.WaitForClick();

                _nicknameInput.interactable = false;
                _login.gameObject.SetActive(false);
                
                await _authService.UpdateNickname(_nicknameInput.text);
            }
            else
            {
                _authService.UpdateNickname(default);
            }

            _nicknameInput.text = $"{AuthService.Nickname}";
            
            await _saveLoadService.Load();

            Instantiate(_lobbyConnector);
        }
    }
}