using Dev.Infrastructure;
using Dev.Utils;
using Dev.Weapons.Guns;
using Zenject;

namespace Dev.Levels
{
    public class DummyTarget : NetworkContext, IDamageable
    {
        private HealthObjectsService _healthObjectsService;
        public int DamageId => AtomicConstants.DamageIds.ObstacleDamageId;


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