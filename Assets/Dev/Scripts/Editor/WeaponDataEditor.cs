using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Weapons.StaticData;
using UnityEditor;
using UnityEngine;

namespace Dev.Editor
{
    public class WeaponDataEditor : EditorWindow
    {
        private WeaponStaticData _currentWeapon;    
        private SerializedObject _currentWeaponSerialized;
        private int _selectedDataIndex;

        private List<WeaponStaticData> _weaponsData = new List<WeaponStaticData>();
        
        [MenuItem("Tools/WeaponDataEditor")]
        private static void ShowWindow()
        {
            var window = GetWindow<WeaponDataEditor>();
            window.titleContent = new GUIContent("Weapon Data Editor");
            window.Show();
        }

        private void CreateGUI()
        {
            string[] guids = AssetDatabase.FindAssets("t:WeaponStaticData", new string[] { "Assets/Dev/SO/Weapons" });

            _weaponsData.Clear();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                WeaponStaticData so = AssetDatabase.LoadAssetAtPath<WeaponStaticData>(path);
                if (so != null)
                {
                    _weaponsData.Add(so);
                }
            }

            // Создаем массив строк с именами для отображения в дропдауне
            //objectNames = allScriptableObjects.Select(so => so.name).ToArray();
        }

        private void OnDisable()
        {
            _weaponsData.Clear();
        }

        private void OnGUI()
        {
            if (_weaponsData.Count == 0)
            {
                EditorGUILayout.HelpBox("Data will be found only on 'Assets/Dev/SO/Weapons' path", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Choose weapon data", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            _selectedDataIndex = EditorGUILayout.Popup("WeaponData", _selectedDataIndex, _weaponsData.Select(so => so.name).ToArray());
            _currentWeapon = _weaponsData[_selectedDataIndex];
            if(_currentWeapon == null) return;

            EditorGUILayout.Space(20);
            
            if (_currentWeaponSerialized == null || _currentWeaponSerialized.targetObject != _currentWeapon)
                _currentWeaponSerialized = new SerializedObject(_currentWeapon);
            
            SerializedProperty prop = _currentWeaponSerialized.GetIterator();
            prop.NextVisible(true); // Пропускаем первую строку (script)

            while (prop.NextVisible(false))
            {
                EditorGUILayout.PropertyField(prop, true);
            }

            if (GUI.changed)
            {
                _currentWeaponSerialized.ApplyModifiedProperties();
            }
        }
    }
}