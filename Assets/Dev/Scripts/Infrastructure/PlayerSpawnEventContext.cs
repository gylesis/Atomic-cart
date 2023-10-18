using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    public struct PlayerSpawnEventContext
    {
        public CharacterClass CharacterClass;
        public PlayerRef PlayerRef;
        public Transform Transform;
    }
}