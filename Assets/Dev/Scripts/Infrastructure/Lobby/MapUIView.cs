using Dev.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.Infrastructure
{
    public class MapUIView : UIElement<MapUIView>
    {
        [SerializeField] private Image _mapIcon;
        [SerializeField] private TMP_Text _mapName;

        public MapType MapType { get; private set; }
        
        public string MapName { get; private set; }

        public void Init(MapUIViewSetupContext setupContext)
        {
            MapType = setupContext.MapType;
            _mapIcon.sprite = setupContext.MapIcon;
            MapName = setupContext.MapName;
            _mapName.text = MapName;
        }

    }
}