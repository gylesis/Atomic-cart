﻿using Dev.Effects;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.Levels.Interactions.Pickable
{
    public class HealthPickableObject : PickableObject
    {
        [SerializeField] private int _healthRestoreAmount;
        
        private PlayersDataService _playersDataService;

        [Inject]
        private void Construct(PlayersDataService playersDataService)
        {
            _playersDataService = playersDataService;
        }
        
        protected override void OnAutoInteraction(PlayerRef interactedPlayer)
        {
            base.OnAutoInteraction(interactedPlayer);
            PlayersHealthService.Instance.GainHealthToPlayer(interactedPlayer, _healthRestoreAmount);

            Vector3 playerPos = _playersDataService.GetPlayerPos(interactedPlayer);

            FxController.Instance.SpawnEffectAt("picked_health", playerPos);

            Debug.Log($"Player picked health for {_healthRestoreAmount}", gameObject);
        }
    }
}