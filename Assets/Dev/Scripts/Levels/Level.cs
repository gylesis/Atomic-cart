using Dev.Infrastructure;
using UnityEngine;

namespace Dev
{
    public class Level : NetworkContext
    {
        [SerializeField] private SpawnPoint[] _spawnPoints;

        public SpawnPoint[] SpawnPoints => _spawnPoints;
    }
}