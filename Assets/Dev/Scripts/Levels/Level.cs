using System.Collections.Generic;
using System.Linq;
using Dev.CartLogic;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using UnityEngine;

namespace Dev.Levels
{
    public class Level : NetworkContext
    {
        [SerializeField] private SpawnPoint[] _redTeamSpawnPoints;
        [SerializeField] private SpawnPoint[] _blueTeamSpawnPoints;

        [SerializeField] private CartPathService _cartPathService;
        
        private List<Obstacle> _obstacles;
        private List<InteractionObject> _interactionObjects;

        public List<InteractionObject> InteractionObjects => _interactionObjects;
        public List<Obstacle> Obstacles => _obstacles;

        public CartPathService CartPathService => _cartPathService;

        private void Awake()
        {
            _obstacles = GetComponentsInChildren<Obstacle>(true).ToList();
            _interactionObjects = GetComponentsInChildren<InteractionObject>(true).ToList();
        }

        public SpawnPoint[] GetSpawnPointsByTeam(TeamSide teamSide)
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            foreach (SpawnPoint point in _redTeamSpawnPoints)
            {
                Gizmos.DrawSphere(point.transform.position, 0.2f);
            }

            Gizmos.color = Color.blue;

            foreach (SpawnPoint point in _blueTeamSpawnPoints)
            {
                Gizmos.DrawSphere(point.transform.position, 0.2f);
            }
        }
    }
}