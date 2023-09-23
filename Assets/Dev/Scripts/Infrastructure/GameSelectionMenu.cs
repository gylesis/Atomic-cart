using System;
using Dev.UI;
using Zenject;

namespace Dev.Infrastructure
{
    public class GameSelectionMenu : PopUp
    {
        private MapsContainer _mapsContainer;

        [Inject]
        private void Init(MapsContainer mapsContainer)
        {
            _mapsContainer = mapsContainer;
        }

        private void Start()
        {
            foreach (MapData mapData in _mapsContainer.MapDatas)
            {
                
            }
        }
    }
    
    
    
    
}