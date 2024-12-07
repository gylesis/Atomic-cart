using System;
using System.Collections.Generic;
using Dev.Infrastructure;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dev.UI.PopUpsAndMenus
{
    public class PopUpService
    {
        public static PopUpService Instance { get; private set; }
        public Subject<PopUpStateContext> PopUpStateChanged { get; } = new Subject<PopUpStateContext>();

        private Queue<PopUp> _popUpsChain = new Queue<PopUp>(); // TODO implement chaining resolve when pressing back
        private Dictionary<Type, PopUp> _spawnedPopUps = new Dictionary<Type, PopUp>();
        private List<Type> _scenePopUps = new ();
        private Transform _linkedScenePopUpsParent;
        private CompositeDisposable _disposable = new CompositeDisposable();

        private DiInjecter _diInjecter;
        private Transform _spawnedPopUpsParent;
        private GameSettings _gameSettings;

        private PopUpService(GameSettings gameSettings, DiInjecter diInjecter, Transform spawnedPopUpsParent)
        {
            _gameSettings = gameSettings;
            _spawnedPopUpsParent = spawnedPopUpsParent;
            _diInjecter = diInjecter;

            Instance = this;
        }

        public void UpdateSceneLink(Transform linkedTransform)
        {
            //_diInjecter.InjectGameObject(linkedTransform.gameObject);

            foreach (var type in _scenePopUps)
            {
                _spawnedPopUps.Remove(type);
            }
            
            _linkedScenePopUpsParent = linkedTransform;
            LinkPopUps();
        }

        private void LinkPopUps()
        {
            var popUps = _linkedScenePopUpsParent.GetComponentsInChildren<PopUp>();

            foreach (PopUp popUp in popUps)
            {
                popUp.InitPopUpService(this);
                Type type = popUp.GetType();

                popUp.ShowAndHide.Subscribe((b => OnPopUpStateChanged(type, b))).AddTo(_disposable);
                _spawnedPopUps.Add(type, popUp);
                _scenePopUps.Add(type);
            }
        }

        private void OnPopUpStateChanged(Type type, bool isOn)
        {
            var stateContext = new PopUpStateContext();
            stateContext.IsOn = isOn;
            stateContext.PopUpType = type;

            if (isOn)
            {
                _popUpsChain.Enqueue(_spawnedPopUps[type]);
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

        private void TryGetPopUp<TPopUp>(out TPopUp popUp) where TPopUp : PopUp
        {
            Type popUpType = typeof(TPopUp);

            if (_spawnedPopUps.TryGetValue(popUpType, out var spawnedPopUp))
            {
                popUp = spawnedPopUp as TPopUp;
            }
            else
            {
                popUp = SpawnPopUp<TPopUp>();
                _spawnedPopUps.Add(popUpType, popUp);
            }
        }

        public TPopUp ShowPopUp<TPopUp>(Action onSuccedBtnClicked = null) where TPopUp : PopUp
        {
            TryGetPopUp<TPopUp>(out var popUp);

            popUp.OnSucceedButtonClicked(onSuccedBtnClicked);
            popUp.Show();
            return popUp;
        }

        public void HidePopUp<TPopUp>() where TPopUp : PopUp
        {
            TryGetPopUp<TPopUp>(out var popUp);

            popUp.Hide();
        }

        public void HidePopUp(PopUp popUp)
        {
            popUp.Hide();
        }

        private TPopUp SpawnPopUp<TPopUp>() where TPopUp : PopUp
        {
            var popUp = Object.Instantiate(GetPrefab<TPopUp>(), _spawnedPopUpsParent);
            popUp.InitPopUpService(this);
            _diInjecter.InjectGameObject(popUp.gameObject);
            return popUp;
        }

        private TPopUp GetPrefab<TPopUp>() where TPopUp : PopUp
        {
            return _gameSettings.PopUpsStaticDataContainer.GetPrefab<TPopUp>();
        }

        private bool IsPopUpSpawned<TPopUp>() => _spawnedPopUps.ContainsKey(typeof(TPopUp));

        public void HideAllPopUps()
        {
            foreach (var popUp in _spawnedPopUps)
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