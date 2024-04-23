using System.Collections.Generic;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class PlayerManager
    {
        private static List<PlayerCharacter> _allPlayers = new List<PlayerCharacter>();
       
        public static List<PlayerRef> PlayersOnServer = new List<PlayerRef>();
        public static List<PlayerRef> LoadingPlayers = new List<PlayerRef>();

        
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