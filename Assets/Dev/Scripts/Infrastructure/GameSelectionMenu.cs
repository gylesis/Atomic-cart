using System;
using System.Collections.Generic;
using System.Linq;
using Dev.UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class GameSelectionMenu : PopUp
    {
        [SerializeField] private MapUIView _mapUIViewPrefab;
        [SerializeField] private Transform _mapParent;

        [SerializeField] private UIElementsGroup _uiElementsGroup;
        
        private MapsContainer _mapsContainer;

        private List<MapUIView> _mapUIViews = new List<MapUIView>(4);

        protected override void Awake()
        {
            
        }

        [Inject]
        private void Init(MapsContainer mapsContainer)
        {
            _mapsContainer = mapsContainer;
        }

        private void Start()
        {
            MapData data = _mapsContainer.MapDatas.First();
            GameStaticData.LevelName = data.Name;
            GameStaticData.MapType = data.MapType;
            
            foreach (MapData mapData in _mapsContainer.MapDatas)
            {
                MapUIView mapUIView = Instantiate(_mapUIViewPrefab, _mapParent);
                var setupContext = new MapUIViewSetupContext();
                setupContext.MapIcon = mapData.MapIcon;
                setupContext.MapName = mapData.Name;
                setupContext.MapType = mapData.MapType;
                
                mapUIView.Init(setupContext);
                
                mapUIView.Clicked.TakeUntilDestroy(this).Subscribe((OnMapUIClicked));
                _mapUIViews.Add(mapUIView);
            }
            
            _uiElementsGroup.Init(_mapUIViews.Select(x => x as UIElementBase).ToList());
            _uiElementsGroup.Select(_mapUIViews.First());
        }

        private void OnMapUIClicked(MapUIView mapUIView)
        {   
            _uiElementsGroup.Select(mapUIView);
            GameStaticData.LevelName = mapUIView.MapName;
        }
    }
}