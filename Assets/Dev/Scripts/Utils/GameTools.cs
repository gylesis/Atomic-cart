#if UNITY_EDITOR
using System.Linq;
using Dev.Infrastructure;
using Dev.Infrastructure.Lobby;
using UnityEditor;
using UnityEngine;

namespace Dev.Utils
{
    public class GameTools
    {
        [MenuItem("Tools/Refresh Maps Enum")]
        public static async void RefreshEnumMaps()
        {
            string filename = "MapName";
            
            string file = "namespace Dev.Utils\n";
            file += "{\n";
            file += $"    public enum {filename}\n";
            file += "    {\n";

            var assets = AssetDatabase.FindAssets("t:MapsContainer", new [] { "Assets/Dev/SO/Maps"});
            string mapsContainerPath = AssetDatabase.GUIDToAssetPath(assets[0]);
            
            MapsContainer mapsContainer = AssetDatabase.LoadAssetAtPath<MapsContainer>(mapsContainerPath);
            
            var levels = mapsContainer.MapDatas.Select(x => x.Name).ToList();

            foreach (var level in levels)
            {
                file += "       " + level + ",\n";
            }
            
            file += "    }\n";
            file += "}";

            string filePath = Application.dataPath + $"/Dev/Scripts/Utils/{filename}.cs";
            System.IO.File.WriteAllText(filePath, file);

            AssetDatabase.Refresh();
        }
        
    }
}
#endif