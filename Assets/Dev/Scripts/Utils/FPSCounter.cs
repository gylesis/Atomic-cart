using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Dev.Utils
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private int _targetFPS = -1;

        private readonly Queue<float> _fpsValues = new Queue<float>();

        private void Awake()
        {
/*#if UNITY_EDITOR
            Application.targetFrameRate = -1;
#else
            Application.targetFrameRate = _targetFPS;
#endif*/
            
            Application.targetFrameRate = 300;
        }

        private void OnGUI()
        {
            if (_fpsValues.Count > 30)
            {
                _fpsValues.Dequeue();
            }

            float currentFPs = 1f / Time.deltaTime;

            _fpsValues.Enqueue(currentFPs);

            float averageFPS = -1;

            foreach (float fpsValue in _fpsValues)
            {
                averageFPS += fpsValue;
            }

            averageFPS /= _fpsValues.Count;

            GUIStyle guiStyle = new GUIStyle();
            guiStyle.fontSize = 35;
            guiStyle.fontStyle = FontStyle.Bold;
            guiStyle.active = new GUIStyleState
            {
                textColor = Color.white
            };

            GUI.Label(new Rect(400, 50, 100, 50), $"FPS:{(int)averageFPS}", guiStyle);
        }
    }
}