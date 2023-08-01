using Dev.Infrastructure;
using UnityEngine;

namespace Dev
{
    public class Level : NetworkContext
    {
        [SerializeField] private SpawnPoint[] _redTeamSpawnPoints;
        [SerializeField] private SpawnPoint[] _blueTeamSpawnPoints;
        
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