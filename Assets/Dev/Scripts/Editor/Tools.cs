using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Dev.Scripts.Editor
{
    public class Tools : MonoBehaviour
    {
        private const string AndroidLastBuildVersionKey = "Android_LastBuild_Version";
        
        //[MenuItem("Tools/BuildAndroid)")]
        private static void BuildAndroid()
        {
            string lastVersion;
            
            if (PlayerPrefs.HasKey(AndroidLastBuildVersionKey))
            {
                lastVersion = PlayerPrefs.GetString(AndroidLastBuildVersionKey);
            }
            else
            {
                lastVersion = "0.01";
            }
            
            string buildName = $"{lastVersion}.apk";
            
            string path = $@"E:\Dev\Unity\Projects\Site\atomic cart\AndroidBuilds\{buildName}";

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.locationPathName = path;
            buildPlayerOptions.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeed");

                Debug.Log($"Last version {lastVersion}");
                double version = Convert.ToDouble(lastVersion);
                version += 0.01;
                
                PlayerPrefs.SetString(AndroidLastBuildVersionKey, version.ToString());
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log($"Build result: {summary.result}");
            }   
            
            
        }
        
        
    }
}