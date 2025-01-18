using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure.Networking;
using Dev.UI;
using Dev.Utils;
using TMPro;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class AuthBootstrap : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nicknameInput;
        [SerializeField] private DefaultReactiveButton _login;
        
        private ConnectionManager _connectionManager;
        private InternetChecker _internetChecker;
        private ModulesService _modulesService;

        [Inject]
        private void Construct(InternetChecker internetChecker, ModulesService modulesService, ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _modulesService = modulesService;
            _internetChecker = internetChecker;
        }
        
        private void Start()
        {
            Connect();  
        }

        private async void Connect()
        {
            var initializeResult = await _modulesService.Initialize();

            if (initializeResult.IsError)
            {
                Curtains.Instance.SetText("Error initializing services, retrying...");
                AtomicLogger.Err($"Error initializing services {initializeResult.ErrorMessage}");
                
                await UniTask.Delay(TimeSpan.FromSeconds(5));
                Connect();
                
                return;
            }
            
            await _internetChecker.Check(gameObject.GetCancellationTokenOnDestroy());

            ConnectionManager connectionManager = ConnectionManager.IsInitialized ? ConnectionManager.Instance : Instantiate(this._connectionManager);
            connectionManager.ConnectFromBootstrap();
        }
    }
}