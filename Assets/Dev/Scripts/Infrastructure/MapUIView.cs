using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.Infrastructure
{
    public class MapUIView : UIElement<MapUIView>
    {
        [SerializeField] private Image _mapIcon;
        [SerializeField] private TMP_Text _mapName;

        private MapType _mapType;
        
        public string MapName { get; private set; }

        public void Init(MapUIViewSetupContext setupContext)
        {
            _mapType = setupContext.MapType;
            _mapIcon.sprite = setupContext.MapIcon;
            MapName = setupContext.MapName;
            _mapName.text = MapName;
        }

    }
}