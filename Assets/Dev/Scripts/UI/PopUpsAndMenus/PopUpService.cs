using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    public class PopUpService : MonoBehaviour
    {
        [SerializeField] private Transform _popUpsParent;

        private Dictionary<Type, PopUp> _spawnedPrefabs = new Dictionary<Type, PopUp>();

        public Subject<PopUpStateContext> PopUpStateChanged { get; } = new Subject<PopUpStateContext>();

        private Queue<PopUp> _popUpsChain = new Queue<PopUp>();

        private void Awake()
        {
            var popUps = _popUpsParent.GetComponentsInChildren<PopUp>();

            foreach (PopUp popUp in popUps)
            {
                popUp.InitPopUpService(this);
                Type type = popUp.GetType();

                popUp.ShowAndHide.TakeUntilDestroy(this).Subscribe((b => OnPopUpStateChanged(type, b)));
                _spawnedPrefabs.Add(type, popUp);
            }
        }

        private void OnPopUpStateChanged(Type type, bool isOn)
        {
            var stateContext = new PopUpStateContext();
            stateContext.IsOn = isOn;
            stateContext.PopUpType = type;

            if (isOn)
            {
                _popUpsChain.Enqueue(_spawnedPrefabs[type]);
            }
            else
            {
                if (_popUpsChain.Count != 0)
                {
                    _popUpsChain.Dequeue();
                }
            }


            PopUpStateChanged.OnNext(stateContext);
        }

        public void ClosePrevPopUps(int amount = 1)
        {
            amount = Mathf.Clamp(amount, 0, _popUpsChain.Count);

            for (int i = 0; i < amount; i++)
            {
                PopUp popUp = _popUpsChain.Dequeue();

                popUp.Hide();
            }
        }

        public bool TryGetPopUp<TPopUp>(out TPopUp popUp) where TPopUp : PopUp
        {
            popUp = null;

            Type popUpType = typeof(TPopUp);

            if (_spawnedPrefabs.TryGetValue(popUpType, out PopUp prefab))
            {
                popUp = prefab as TPopUp;
                return popUp;
            }

            _spawnedPrefabs.Add(typeof(TPopUp), popUp);

            Debug.Log($"No such PopUp like {popUpType}");

            return popUp;
        }

        public void ShowPopUp<TPopUp>() where TPopUp : PopUp
        {
            var tryGetPopUp = TryGetPopUp<TPopUp>(out var popUp);

            if (tryGetPopUp)
            {
                popUp.Show();
            }
        }

        public void HidePopUp<TPopUp>() where TPopUp : PopUp
        {
            var tryGetPopUp = TryGetPopUp<TPopUp>(out var popUp);

            if (tryGetPopUp)
            {
                popUp.Hide();
            }
        }

        public void HideAllPopUps()
        {
            foreach (var popUp in _spawnedPrefabs)
            {
                popUp.Value.Hide();
            }
        }
    }

    public struct PopUpStateContext
    {
        public Type PopUpType;
        public bool IsOn;
    }
}