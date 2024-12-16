using System;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Levels.Interactions.Pickable
{
    public abstract class PickableObject : InteractionObject
    {
        [SerializeField] private float _respawnCooldown = 10;
        
        [Networked] private NetworkBool IsPickedUp { get; set; }

        protected override void CorrectState()
        {
            base.CorrectState();
            
            _playerTriggerZone.gameObject.SetActive(!IsPickedUp);
            transform.localScale = IsPickedUp ? Vector3.zero : Vector3.one;
        }

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

        [Rpc(Channel = RpcChannel.Reliable)]
        private void RPC_SetForRespawn()
        {
            _playerTriggerZone.gameObject.SetActive(false);
            IsPickedUp = true;
            RPC_DoScale(0.5f, 0, Ease.OutBounce);
            Observable.Timer(TimeSpan.FromSeconds(_respawnCooldown)).TakeUntilDestroy(this).Subscribe((l =>
            {
                if (Runner.IsSharedModeMasterClient)
                {
                    RPC_Respawn();
                    IsPickedUp = false;
                }
            }));
        }

        [Rpc(Channel = RpcChannel.Reliable)]
        private void RPC_Respawn()
        {
            transform.DOScale(1, 0.5f).SetEase(Ease.InCubic).OnComplete((() => _playerTriggerZone.gameObject.SetActive(true)));
        }
        
    }
}