﻿using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Dev.UI
{
    public class PopUpService : MonoBehaviour
    {
        [SerializeField] private Transform _popUpsParent;

        private Dictionary<Type, PopUp> _popUpsPrefabs;
        private Dictionary<Type, PopUp> _spawnedPrefabs = new Dictionary<Type, PopUp>();

        // public Subject<Unit> PopUpClosed { get; } = new Subject<Unit>();

        public Subject<PopUpStateContext> PopUpStateChanged { get; } = new Subject<PopUpStateContext>();

        private void Awake()
        {
            var popUps = _popUpsParent.GetComponentsInChildren<PopUp>();

            _popUpsPrefabs = popUps.ToDictionary(x => x.GetType());

            foreach (PopUp popUp in popUps)
            {
                popUp.InitPopUpService(this);
                Type type = popUp.GetType();

                popUp.OnHide.TakeUntilDestroy(this).Subscribe((b => OnPopUpStateChanged(type, b)));
                _spawnedPrefabs.Add(type, popUp);
            }
        }

        private void OnPopUpStateChanged(Type type, bool isOn)
        {
            var stateContext = new PopUpStateContext();
            stateContext.IsOn = isOn;
            stateContext.PopUpType = type;
            
            PopUpStateChanged.OnNext(stateContext);
        }


        public bool TryGetPopUp<TPopUp>(out TPopUp popUp) where TPopUp : PopUp
        {
            popUp = null;

            Type popUpType = typeof(TPopUp);

            bool containsKey = _popUpsPrefabs.ContainsKey(popUpType);

            if (containsKey)
            {
                if (_spawnedPrefabs.ContainsKey(popUpType))
                {
                    PopUp spawnedPrefab = _spawnedPrefabs[popUpType];

                    popUp = spawnedPrefab as TPopUp;
                    return popUp;
                }

                _spawnedPrefabs.Add(typeof(TPopUp), popUp);
            }

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