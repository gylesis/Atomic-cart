using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using UnityEngine;

namespace Dev
{
    public class InternetChecker
    {
        private Curtains _curtains;
        private GameSettings _gameSettings;

        private int _nextInternetCheckDelay = 3;
        
        public InternetChecker(Curtains curtains, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _curtains = curtains;
        }
        
        public async UniTask<bool> Check()
        {
            _curtains.SetText("Checking for internet...");
            _curtains.Show(0);

            while (true)
            {
                bool hasInternet = await CheckInternal(_nextInternetCheckDelay);

                if (hasInternet)
                {
                    break;
                }
                else
                {
                    _curtains.SetText("<color=red>No internet connection.</color> Please connect to network");
                }
                
            }
            
            _curtains.Hide();
            return true;
        }


        private async UniTask<bool> CheckInternal(float nextTryDelay)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(nextTryDelay));
                return false;
            }

            return true;
        }
        
    }
}