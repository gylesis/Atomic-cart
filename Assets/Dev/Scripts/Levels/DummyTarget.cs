using Dev.Infrastructure;
using Dev.Weapons.Guns;
using Zenject;

namespace Dev.Levels
{
    public class DummyTarget : NetworkContext, IDamageable
    {
        private HealthObjectsService _healthObjectsService;
        public DamagableType DamageId => DamagableType.Obstacle;

        [Inject]
        private void Construct(HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
        }

        protected override void OnInjectCompleted()
        {
            base.OnInjectCompleted();
            
            _healthObjectsService.RegisterObject(Object, 999);
        }
    }
}