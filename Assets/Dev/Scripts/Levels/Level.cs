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
using UnityEngine.Serialization;
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

        [FormerlySerializedAs("metaData")] [SerializeField] private LevelMetaData _metaData;

        public LevelMetaData MetaData => _metaData;

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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_metaData.Bounds.center, _metaData.Bounds.size);
        }
    }

    [Serializable]
    public class LevelMetaData
    {
        [SerializeField] private Transform _leftUpCorner;
        [SerializeField] private Transform _rightUpCorner;
        [SerializeField] private Transform _leftDownCorner;
        [SerializeField] private Transform _rightDownCorner;
        
        private Bounds _bounds;
        
        public Bounds Bounds
        {
            get
            {
                //if(!_bounds.Equals(default))
               //     return _bounds;
                
                Vector3 center = _leftDownCorner.position + _rightUpCorner.position;
                center /= 2;

                float ySize = (_leftDownCorner.position - _leftUpCorner.position).magnitude;
                float xSize = (_leftDownCorner.position - _rightDownCorner.position).magnitude;
                
                Vector3 size = new Vector3(ySize, xSize, 999);

                _bounds = new Bounds(center, size);
                
                return _bounds;
            }
        }
    }
}