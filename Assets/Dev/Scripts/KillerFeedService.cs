using System;
using Dev.Infrastructure;
using Dev.Utils;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class KillerFeedService : NetworkContext
    {
        private HealthObjectsService _healthObjectsService;
        private KillerFeedNotifyService _killerFeedNotifyService;

        [Inject]
        private void Construct(HealthObjectsService healthObjectsService, KillerFeedNotifyService killerFeedNotifyService)
        {
            _killerFeedNotifyService = killerFeedNotifyService;
            _healthObjectsService = healthObjectsService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _healthObjectsService.PlayerDied.Subscribe((OnUnitDied)).AddTo(this);
            _healthObjectsService.BotDied.Subscribe((OnUnitDied)).AddTo(this);
        }

        private void OnUnitDied(UnitDieContext context)
        {
            string killer = context.IsKilledByServer ? "Server" : context.Killer.Name;
            string victim = context.Victim.Name;
            
            AtomicLogger.Log($"{killer} killed {victim}");
            
            _killerFeedNotifyService.Notify(killer, victim);
        }

    }
}