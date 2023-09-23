using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(menuName = "StaticData/Maps/MapStaticData", fileName = "MapStaticData", order = 0)]
    public class MapData : ScriptableObject
    {
        public string Name;
        public MapType MapType;

        public Sprite MapIcon;
    }
}