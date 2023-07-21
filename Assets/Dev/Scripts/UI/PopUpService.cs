using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Dev.UI
{
    public class PopUpService // i'd like to add event polling while closing popup or menu
    {
        private readonly Dictionary<Type, PopUp> _popUpsPrefabs;
        private readonly Dictionary<Type, PopUp> _spawnedPrefabs = new Dictionary<Type, PopUp>();

        private Transform _parent;

        public Subject<Unit> PopUpClosed { get; } = new Subject<Unit>();

        public PopUpService(PopUp[] popUps, Transform parent)
        {
            _parent = parent;
            _popUpsPrefabs = popUps.ToDictionary(x => x.GetType());

            foreach (PopUp popUp in popUps)
            {
                Type type = popUp.GetType();

                _spawnedPrefabs.Add(type, popUp);
            }
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
    }
}