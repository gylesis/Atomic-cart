using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class PlayerManager : MonoBehaviour
    {
        private static List<Player> _allPlayers = new List<Player>();
        public static List<Player> allPlayers => _allPlayers;

        //private static Queue<Player> _playerQueue = new Queue<Player>();
        public static Queue<PlayerRef> PlayerQueue = new Queue<PlayerRef>();


        public static void AddPlayerForQueue(PlayerRef playerRef)
        {
            PlayerQueue.Enqueue(playerRef);
        }
        
        public static void AddPlayer(Player player)
        {
            Debug.Log("Player Added");

            int insertIndex = _allPlayers.Count;
            // Sort the player list when adding players
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].PlayerRef > player.PlayerRef)
                {
                    insertIndex = i;
                    break;
                }
            }

            _allPlayers.Insert(insertIndex, player);
           // _playerQueue.Enqueue(player);
        }

        public static void RemovePlayer(Player player)
        {
            if (player == null || !_allPlayers.Contains(player))
                return;

            Debug.Log("Player Removed " + player.PlayerRef);

            _allPlayers.Remove(player);
        }

        public static Player Get(PlayerRef playerRef)
        {
            for (int i = _allPlayers.Count - 1; i >= 0; i--)
            {
                if (_allPlayers[i] == null || _allPlayers[i].Object == null)
                {
                    _allPlayers.RemoveAt(i);
                    Debug.Log("Removing null player");
                }
                else if (_allPlayers[i].Object.InputAuthority == playerRef)
                    return _allPlayers[i];
            }

            return null;
        }
    }
}