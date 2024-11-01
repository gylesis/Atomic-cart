using System.Diagnostics;
using System.IO;
using Dev.Utils;
using UnityEditor;

namespace Dev.Editor
{
    public static class Builder
    {
        [MenuItem("Tools/Builder/OpenBuildFolder")]
        public static void OpenBuildFolder()
        {
            string basePath = @"E:\Dev\Unity\Projects\Site\atomic cart\Builds";
            string buildFolder = basePath;
            
#if UNITY_STANDALONE_WIN
            buildFolder += @"\PC";
#elif UNITY_ANDROID
            buildFolder += @"\Android";
#endif
            
            if (Directory.Exists(buildFolder) == false)
            {
                AtomicLogger.Err($"Build folder [{buildFolder}] doesn't exist");
                return;
            }
            
            Process.Start("explorer.exe", buildFolder);
        }
        
        
    }
}