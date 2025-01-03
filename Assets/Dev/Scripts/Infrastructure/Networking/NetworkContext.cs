﻿using DG.Tweening;
using Fusion;
using UnityEditor;
using UnityEngine;

namespace Dev.Infrastructure.Networking
{
    [RequireComponent(typeof(NetworkObject))]
    public abstract class NetworkContext : NetworkBehaviour
    {
        [Networked] public NetworkBool IsActive { get; private set; } = true;

        // CALL ORDER: Awake -> LoadLateInjection -> Spawned -> .....
        
        
        protected virtual void Awake()
        {
            LoadLateInjection();
        }

        protected virtual void Start()
        {
            OnInjectCompleted();
        }

        protected virtual void LoadLateInjection()
        {
            DiInjecter.Instance.InjectGameObject(gameObject);
        }

        /// <summary>
        /// Invokes right after Zenject completed injection [Inject]
        /// </summary>
        protected virtual void OnInjectCompleted() { }

        public override async void Spawned()
        {
            IsActive = true;

            CorrectState();
        }

        [Rpc(Channel = RpcChannel.Reliable)]
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

        [Rpc(Channel = RpcChannel.Reliable)]
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

        [Rpc]
        public void RPC_SetParentAndSetZero(NetworkObject newParent)
        {
            if (newParent == null)
            {
                transform.parent = null;
            }
            else
            {
                transform.parent = newParent.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }


#if UNITY_EDITOR
        [ContextMenu(nameof(SetChildrenMasterClientObject))]
        private void SetChildrenMasterClientObject()
        {
            NetworkObject[] childrens = transform.GetComponentsInChildren<NetworkObject>();

            foreach (NetworkObject networkObject in childrens)
            {
                networkObject.Flags = NetworkObjectFlags.MasterClientObject | NetworkObjectFlags.V1;
                EditorUtility.SetDirty(this);
            }

            Debug.Log($"For {childrens.Length} objects Master Client Object flag was set");
        }

#endif
    }
}