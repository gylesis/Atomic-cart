using System.Collections.Generic;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class PlayerManager : MonoBehaviour
    {
        private static List<PlayerCharacter> _allPlayers = new List<PlayerCharacter>();
        public static List<PlayerCharacter> AllPlayers => _allPlayers;

        //private static Queue<Player> _playerQueue = new Queue<Player>();
        public static Queue<PlayerRef> PlayerQueue = new Queue<PlayerRef>();


        public static void AddPlayerForQueue(PlayerRef playerRef)
        {
            PlayerQueue.Enqueue(playerRef);
        }

        public static void AddPlayer(PlayerCharacter playerCharacter)
        {
            return;
            //Debug.Log("Player Added");

            int insertIndex = _allPlayers.Count;
            // Sort the player list when adding players
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].DamageId > playerCharacter.DamageId)
                {
                    insertIndex = i;
                    break;
                }
            }

            _allPlayers.Insert(insertIndex, playerCharacter);
            // _playerQueue.Enqueue(player);
        }

        public static void RemovePlayer(PlayerCharacter playerCharacter)
        {
            return;
            if (playerCharacter == null || !_allPlayers.Contains(playerCharacter))
                return;

            Debug.Log("Player Removed " + playerCharacter.DamageId);

            _allPlayers.Remove(playerCharacter);
        }

        public static PlayerCharacter Get(PlayerRef playerRef)
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