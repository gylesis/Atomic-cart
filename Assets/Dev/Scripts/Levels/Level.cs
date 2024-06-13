using System.Collections.Generic;
using System.Linq;
using Dev.BotsLogic;
using Dev.CartLogic;
using Dev.Infrastructure;
using Dev.Levels.Interactions;
using Dev.PlayerLogic;
using UnityEngine;
using Zenject;

namespace Dev.Levels
{
    public class Level : NetworkContext
    {
        [SerializeField] private Transform _botMovePointsParent;
        
        [SerializeField] private Transform _redTeamSpawnPointsParent;
        [SerializeField] private Transform _blueTeamSpawnPointsParent;
        
        
        private List<SpawnPoint> _redTeamSpawnPoints;
        private List<SpawnPoint> _blueTeamSpawnPoints;
        
        private List<Obstacle> _obstacles;
        private List<InteractionObject> _interactionObjects;
        private List<BotMovePoint> _botMovePoints;

        private CartPathService _cartPathService;
        
        public List<InteractionObject> InteractionObjects => _interactionObjects;
        public List<Obstacle> Obstacles => _obstacles;

        public List<BotMovePoint> BotMovePoints => _botMovePoints;

        public CartPathService CartPathService => _cartPathService;

        private void Awake()
        {
            _obstacles = GetComponentsInChildren<Obstacle>(true).ToList();
            _interactionObjects = GetComponentsInChildren<InteractionObject>(true).ToList();

            _botMovePoints = _botMovePointsParent.GetComponentsInChildren<BotMovePoint>().ToList();
            
            _redTeamSpawnPoints = _redTeamSpawnPointsParent.GetComponentsInChildren<SpawnPoint>().ToList();
            _blueTeamSpawnPoints = _blueTeamSpawnPointsParent.GetComponentsInChildren<SpawnPoint>().ToList();
        }

        [Inject]
        private void Construct(CartPathService cartPathService)
        {
            _cartPathService = cartPathService;
        }

        public List<SpawnPoint> GetSpawnPointsByTeam(TeamSide teamSide)
        {
            switch (teamSide)
            {
                case TeamSide.Blue:
                    return _blueTeamSpawnPoints;
                case TeamSide.Red:
                    return _redTeamSpawnPoints;
            }

            return null;
        }
    }
}