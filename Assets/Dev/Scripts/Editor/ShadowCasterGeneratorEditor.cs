using Dev.Utils;
using UnityEditor;
using UnityEngine;

namespace Dev.Editor
{
    [CustomEditor(typeof(ShadowCaster2DCreator))]
    public class ShadowCaster2DTileMapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                var creator = (ShadowCaster2DCreator)target;
                creator.Create();
            }

            if (GUILayout.Button("Remove Shadows"))
            {
                var creator = (ShadowCaster2DCreator)target;
                creator.DestroyOldShadowCasters();
            }
            EditorGUILayout.EndHorizontal();
        }

    }

}