using System;
using Dev.Effects;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev
{
    public abstract class PickableObject : InteractionObject
    {
        [SerializeField] private float _respawnCooldown = 10;
        
        [Networked] private NetworkBool IsPickedUp { get; set; }

        protected override void OnAutoInteraction(PlayerRef interactedPlayer)
        {
            RPC_SetForRespawn();
            base.OnAutoInteraction(interactedPlayer);
        }

        protected override void OnInteraction(bool interaction, PlayerRef interactedPlayer)
        {
            if (interactedPlayer != PlayerRef.None)
            {
                RPC_SetForRespawn();
            }

            base.OnInteraction(interaction, interactedPlayer);
        }

        [Rpc]
        private void RPC_SetForRespawn()
        {
            IsPickedUp = true;
            RPC_DoScale(0.5f, 0, Ease.OutBounce);
            Observable.Timer(TimeSpan.FromSeconds(_respawnCooldown)).Subscribe((l =>
            {
                if (Runner.IsSharedModeMasterClient)
                {
                    RPC_Respawn();
                    IsPickedUp = false;
                }
            }));
            
        }

        [Rpc]
        private void RPC_Respawn()
        {
            transform.DOScale(1, 0.5f).SetEase(Ease.InCubic);
        }
        
    }
}