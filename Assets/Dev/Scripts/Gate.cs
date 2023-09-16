using System.Collections.Generic;
using Dev.Levels;
using UnityEngine;

namespace Dev
{
    public class Gate : InteractionObject
    {
        [SerializeField] private List<Obstacle> _obstacles;
        
        protected override void OnInteraction(bool interaction)
        {
            _obstacles.ForEach(x =>
            {
                x.RPC_SetActive(interaction);
            });   
        }
    }
}