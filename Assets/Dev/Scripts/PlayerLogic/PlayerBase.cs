using Dev.Infrastructure;
using Dev.Weapons;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayerBase : NetworkContext
    {
        [Networked, CanBeNull, HideInInspector] public PlayerCharacter Character { get; set; }
        public InputService InputService { get; private set; }
        public AbilityCastController AbilityCastController { get; private set; }

        [Networked] public CharacterClass CharacterClass { get; set; }
        
        public PlayerController PlayerController { get; private set; }
        
        public static PlayerBase LocalPlayerBase;
        
        public override void Spawned()
        {
            base.Spawned();
            
            if (HasStateAuthority) 
                LocalPlayerBase = this;
        }   
      
        
        [Inject]
        private void Construct(InputService inputService, AbilityCastController abilityCastController, PlayerController playerController)
        {
            AbilityCastController = abilityCastController;
            InputService = inputService;
            PlayerController = playerController;
        }
        
        
    }
}