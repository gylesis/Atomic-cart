﻿using Dev.Levels;
using UnityEngine;

namespace Dev.Infrastructure.Lobby
{
    [CreateAssetMenu(menuName = "StaticData/Maps/MapStaticData", fileName = "MapStaticData", order = 0)]
    public class MapData : ScriptableObject
    {
        public string Name;
        public MapType MapType;
        public Sprite MapIcon;
        public Level Prefab;
        
        public bool SupportLighting;
    }
}