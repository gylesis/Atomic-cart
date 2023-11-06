using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerBase : NetworkContext
    {
        [HideInInspector] [Networked] public PlayerCharacter PlayerCharacterInstance { get; set; }

    }
}