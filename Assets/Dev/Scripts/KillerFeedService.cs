using System;
using Dev.Infrastructure;
using Fusion;
using UniRx;
using Zenject;

namespace Dev
{
    public class KillerFeedService : NetworkContext
    {
        [Networked, Capacity(4)] private NetworkLinkedList<HitRecord> HitRecords { get; }

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
            
            _killerFeedNotifyService.Notify(killer, victim);
        }

        public void OnHit(SessionPlayer owner, SessionPlayer victim, float damage, DateTime time)
        {
            HitRecord hitRecord = new HitRecord(owner, victim, damage, time.ToFileTime(), false);
            HitRecords.Add(hitRecord);
        }

        public void OnServerHit(SessionPlayer victim, float damage, DateTime time)
        {
            HitRecord hitRecord = new HitRecord(SessionPlayer.Default, victim, damage, time.ToFileTime(), true);
            HitRecords.Add(hitRecord);
        }
    }
}