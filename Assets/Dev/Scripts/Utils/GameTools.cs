#if UNITY_EDITOR
using System.Linq;
using Dev.Infrastructure;
using UnityEditor;
using UnityEngine;

namespace Dev.Utils
{
    public class GameTools
    {
        [MenuItem("Tools/Refresh Maps Enum")]
        public static async void RefreshEnumMaps()
        {
            string enumCode = "namespace Dev.Utils\n{\n";
            enumCode += "    public enum MapName\n    {\n";

            var assets = AssetDatabase.FindAssets("t:MapsContainer", new [] { "Assets/Dev/SO/Maps"});
            string mapsContainerPath = AssetDatabase.GUIDToAssetPath(assets[0]);
            
            MapsContainer mapsContainer = AssetDatabase.LoadAssetAtPath<MapsContainer>(mapsContainerPath);
            
            var levels = mapsContainer.MapDatas.Select(x => x.Name).ToList();

            foreach (var level in levels)
            {
                enumCode += "        " + level + ",\n";
            }
            
            enumCode += "    }\n";
            enumCode += "}";

            string filePath = Application.dataPath + "/Dev/Scripts/Utils/MapName.cs";
            System.IO.File.WriteAllText(filePath, enumCode);

            AssetDatabase.Refresh();
        }
        
    }
}
#endif