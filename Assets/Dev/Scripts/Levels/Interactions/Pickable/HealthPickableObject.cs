using Dev.Effects;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.Levels.Interactions.Pickable
{
    public class HealthPickableObject : PickableObject
    {
        [SerializeField] private int _healthRestoreAmount;
        
        private PlayersDataService _playersDataService;
        private HealthObjectsService _healthObjectsService;

        [Inject]
        private void Construct(PlayersDataService playersDataService, HealthObjectsService healthObjectsService)
        {
            _healthObjectsService = healthObjectsService;
            _playersDataService = playersDataService;
        }
        
        protected override void OnAutoInteraction(PlayerRef interactedPlayer)
        {
            bool isFullHealth = _healthObjectsService.IsFullHealth(interactedPlayer.ToNetworkId());

            if (isFullHealth)
            {
                Debug.Log($"Player has full hp, skipping health box");
                return;
            }
            
            base.OnAutoInteraction(interactedPlayer);
            _healthObjectsService.GainHealthToPlayer(interactedPlayer, _healthRestoreAmount);

            Vector3 playerPos = _playersDataService.GetPlayerPos(interactedPlayer);

            FxController.Instance.SpawnEffectAt<Effect>("picked_health", playerPos);

            Debug.Log($"Player picked health for {_healthRestoreAmount}", gameObject);
        }
    }
}