using Dev.Infrastructure;
using Dev.Weapons.Guns;
using Zenject;

namespace Dev.Levels
{
    public class DummyTarget : NetworkContext, IDamageable
    {
        private HealthObjectsService _healthObjectsService;
        public DamagableType DamageId => DamagableType.DummyTarget;

        [Inject]
        private void Construct(HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
        }

        public override void Spawned()
        {
            base.Spawned();
            _healthObjectsService.RegisterObject(Object.Id, 999);
        }
    }
}