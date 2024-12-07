using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerScoreUIContainer : NetworkContext
    {
        [SerializeField] private Transform _scoresUIParent;

        public Transform ScoresUIParent => _scoresUIParent;
    }
}