using Dev.Effects;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

namespace Dev.Levels.Interactions.Pickable
{
    public class HealthPickableObject : PickableObject
    {
        [SerializeField] private int _healthRestoreAmount;
        
        private PlayersSpawner _playersSpawner;

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