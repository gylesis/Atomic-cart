using System;
using System.Collections.Generic;
using System.Linq;
using Dev.BotsLogic;
using Dev.CartLogic;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.Levels.Interactions;
using Dev.PlayerLogic;
using Dev.Utils;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.Levels
{
    public class Level : NetworkContext
    {
        [SerializeField] private Transform _botMovePointsParent;
        
        [SerializeField] private Transform _redTeamSpawnPointsParent;
        [SerializeField] private Transform _blueTeamSpawnPointsParent;

        [SerializeField] private List<LightSource> _lightSources;
            
        [Networked] private NetworkString<_16> _levelName { get; set; }
        
        public string LevelName
        {
            get => _levelName.Value;
            set => _levelName = value;
        }

        private List<SpawnPoint> _redTeamSpawnPoints;
        private List<SpawnPoint> _blueTeamSpawnPoints;
        
        private List<Obstacle> _obstacles;
        private List<InteractionObject> _interactionObjects;
        private List<BotMovePoint> _botMovePoints;

        private CartService _cartService;
        
        public List<InteractionObject> InteractionObjects => _interactionObjects;
        public List<Obstacle> Obstacles => _obstacles;

        public List<BotMovePoint> BotMovePoints => _botMovePoints;

        public List<LightSource> LightSources => _lightSources;

        public CartService CartService => _cartService;

        private void Awake()
        {
            _obstacles = GetComponentsInChildren<Obstacle>(true).ToList();
            _interactionObjects = GetComponentsInChildren<InteractionObject>(true).ToList();

            _lightSources = GetComponentsInChildren<LightSource>(true).ToList();
            
            _botMovePoints = _botMovePointsParent.GetComponentsInChildren<BotMovePoint>().ToList();
            
            _redTeamSpawnPoints = _redTeamSpawnPointsParent.GetComponentsInChildren<SpawnPoint>().ToList();
            _blueTeamSpawnPoints = _blueTeamSpawnPointsParent.GetComponentsInChildren<SpawnPoint>().ToList();
        }

        [Inject]
        private void Construct(CartService cartService)
        {
            _cartService = cartService;
        }

        public List<SpawnPoint> GetSpawnPointsByTeam(TeamSide teamSide)
        {
            switch (teamSide)
            {
                case TeamSide.Blue:
                    return _blueTeamSpawnPoints;
                case TeamSide.Red:
                    return _redTeamSpawnPoints;
                default:
                    AtomicLogger.Err("Unknown team side");
                    return _redTeamSpawnPoints;
            }
        }

    }
}