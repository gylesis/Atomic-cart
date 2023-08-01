using System;
using Dev.Infrastructure;
using UniRx;
using UnityEngine;

namespace Dev
{
    public class GameService : NetworkContext
    {
        private TimeService _timeService;
        private PlayersSpawner _playersSpawner;
        private CartPathService _cartPathService;

        private void Awake()
        {
            _timeService = FindObjectOfType<TimeService>();
            _playersSpawner = FindObjectOfType<PlayersSpawner>();
            _cartPathService = FindObjectOfType<CartPathService>();
        }


        public override void Spawned()
        {
            if(HasStateAuthority == false) return;

            _timeService.GameTimeRanOut.TakeUntilDestroy(this).Subscribe((unit => OnGameTimeRanOut()));
        }

        [ContextMenu("Restart")]
        private void OnGameTimeRanOut()
        {
            SetEnemiesFreezeState(true);
            
            _cartPathService.ResetCart();

            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe((l =>
            {
                RespawnAllPlayers();
                SetEnemiesFreezeState(false);
                _timeService.ResetTimer();
            }));
        }

        private void SetEnemiesFreezeState(bool toFreeze)
        {
            foreach (Player player in _playersSpawner.Players)
            {
                player.PlayerController.AllowToMove = !toFreeze;
                player.PlayerController.AllowToShoot = !toFreeze;
            }
        }

        private void RespawnAllPlayers()
        {
            foreach (Player player in _playersSpawner.Players)
            {
                _playersSpawner.RespawnPlayer(player.Object.InputAuthority);
            }
        }
        
        
    }
}