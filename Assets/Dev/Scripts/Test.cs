using System;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class Test : MonoBehaviour
    {
        /*[SerializeField] private string _password;
        
        [ContextMenu("Connect")]
        private async void Connect()
        {
            var appOptions = new AppOptions();
            appOptions.AppId = "1:837773216849:android:33df135842cbce86c1bc70";
            appOptions.ApiKey = "AIzaSyD_hY2NgHyF1l-0I2mqY92SOzTkTLQs3JY";
            appOptions.ProjectId = "atomic-cart";
            
            FirebaseApp firebaseApp = FirebaseApp.Create(appOptions);

            FirebaseAuth firebaseAuth = FirebaseAuth.GetAuth(firebaseApp);

            try
            {
                Debug.Log($"Try to signing");
                AuthResult authResult = await firebaseAuth.SignInWithEmailAndPasswordAsync("qwerty@gmail.com", _password);
            }   
            catch (Exception e)
            {
                Debug.Log($"{e}");
                return;
            }

            Debug.Log($"Sign is Success!");

            var runner = gameObject.AddComponent<NetworkRunner>();

            StartGameResult startGameResult = await runner.JoinSessionLobby(SessionLobby.ClientServer);

            Debug.Log($"Connected to photon");
        }*/
    }
}