using DG.Tweening;
using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    [RequireComponent(typeof(NetworkObject))]
    public abstract class NetworkContext : NetworkBehaviour
    {
        [Networked]
        public NetworkBool IsActive { get; private set; } = true;

        public override void Spawned()
        {
            IsActive = true;
            CorrectState();
            ServerSubscriptions();
        }

        protected virtual void ServerSubscriptions()
        {
            if (HasStateAuthority == false) return;
        }

        [Rpc]
        public void RPC_SetActive(bool isActive)
        {
            IsActive = isActive;
            gameObject.SetActive(isActive);
        }
        
        /// <summary>
        /// Method for restoring state for new clients who connected after changing state happened
        /// </summary>
        protected virtual void CorrectState()
        {
            gameObject.SetActive(IsActive);
        }

        [Rpc]
        public void RPC_SetPos(NetworkObject networkObject, Vector3 pos)
        {
            networkObject.transform.position = pos;
        }

        [Rpc]
        public void RPC_SetPos(Vector3 pos)
        {
            transform.position = pos;
        }

        [Rpc]
        public void RPC_SetLocalPos(NetworkObject networkObject, Vector3 pos)
        {
            networkObject.transform.localPosition = pos;
        }

        [Rpc]
        public void RPC_SetLocalPos(Vector3 pos)
        {
            transform.localPosition = pos;
        }

        [Rpc]
        public void RPC_SetRotation(NetworkObject networkObject, Vector3 eulerAngles)
        {
            networkObject.transform.rotation = Quaternion.Euler(eulerAngles);
        }

        [Rpc]
        public void RPC_SetRotation(Vector3 eulerAngles)
        {
            transform.rotation = Quaternion.Euler(eulerAngles);
        }

        [Rpc]
        public void RPC_SetLocalRotation(Vector3 eulerAngles)
        {
            transform.localRotation = Quaternion.Euler(eulerAngles);
        }

        [Rpc]
        public void RPC_SetName(NetworkObject networkObject, string str)
        {
            networkObject.gameObject.name = str;
        }

        [Rpc]
        public void RPC_SetName(string str)
        {
            gameObject.name = str;
        }

        [Rpc]
        public void RPC_DoScale(NetworkObject networkObject, float duration)
        {
            networkObject.transform.DOScale(1, duration);
        }

        [Rpc]
        public void RPC_DoScale(float duration, float targetValue = 1, Ease ease = Ease.Linear)
        {
            transform.DOScale(targetValue, duration).SetEase(ease);
        }

        [Rpc]
        public void RPC_SetParent(NetworkObject networkObject, NetworkObject newParent)
        {
            if (newParent == null)
            {
                networkObject.transform.parent = null;
            }
            else
            {
                networkObject.transform.parent = newParent.transform;
            }
        }

        [Rpc]
        public void RPC_SetParent(NetworkObject newParent)
        {
            if (newParent == null)
            {
                transform.parent = null;
            }
            else
            {
                transform.parent = newParent.transform;
            }
        }
    }
}