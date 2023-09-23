using System.Collections.Generic;
using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(menuName = "StaticData/Maps/MapsContainer", fileName = "MapsContainer", order = 0)]
    public class MapsContainer : ScriptableObject
    {
        [SerializeField] private List<MapData> _mapDatas;

        public List<MapData> MapDatas => _mapDatas;
    }

}