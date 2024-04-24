using Dev.Infrastructure;
using UnityEngine;

namespace Dev
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private LobbyConnector _lobbyConnector;

        private void Awake()
        {
            Instantiate(_lobbyConnector);
        }
    }
}