using System;
using System.Threading;
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
        
        public async UniTask<bool> Check(CancellationToken cancellationToken)
        {
            while (true)
            {
                _curtains.SetText("Checking for internet");
                _curtains.ShowWithDotAnimation(0);
                
                bool hasInternet = await CheckInternal(_nextInternetCheckDelay, cancellationToken);

                _curtains.StopDotAnimation();
                
                if (hasInternet == false)
                {
                    _curtains.SetText("No internet connection. Please connect to network");
                    _curtains.SetTextColor(Color.red);
                    
                    continue;
                }
                
                break;

            }
            
            _curtains.Hide(0);
            return true;
        }


        private async UniTask<bool> CheckInternal(float nextTryDelay, CancellationToken cancellationToken)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(nextTryDelay)).AttachExternalCancellation(cancellationToken);
                return false;
            }

            return true;
        }
        
    }
}