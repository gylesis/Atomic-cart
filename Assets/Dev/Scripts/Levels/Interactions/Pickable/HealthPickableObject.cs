using Dev.Effects;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class HealthPickableObject : PickableObject
    {
        [SerializeField] private int _healthRestoreAmount;
        
        private PlayersSpawner _playersSpawner;

        public void Init()
        {
            
        }


        protected override void OnAutoInteraction(PlayerRef interactedPlayer)
        {
            base.OnAutoInteraction(interactedPlayer);
            PlayersHealthService.Instance.GainHealthToPlayer(interactedPlayer, _healthRestoreAmount);

            if (_playersSpawner == null)
            {
                _playersSpawner = DependenciesContainer.Instance.GetDependency<PlayersSpawner>();
            }

            Vector3 playerPos = _playersSpawner.GetPlayerPos(interactedPlayer);

            FxController.Instance.SpawnEffectAt("picked_health", playerPos);

            Debug.Log($"Player picked health for {_healthRestoreAmount}", gameObject);
        }
    }
}