using System.Collections.Generic;
using Dev.Levels;
using Fusion;
using UnityEngine;

namespace Dev
{
    public class Gate : InteractionObject
    {
        [SerializeField] private List<Obstacle> _obstacles;
        
        protected override void OnInteraction(bool interaction, PlayerRef interactedPlayer)
        {
            _obstacles.ForEach(x =>
            {
                x.RPC_SetActive(interaction);
            });   
        }
    }
}