using System.Collections.Generic;
using UnityEngine;

namespace Dev.Utils
{
    public class FPSCounter : MonoBehaviour
    {

        [SerializeField] private float _xPos = 400;
        [SerializeField] private float _yPos = 50;
        [SerializeField] private int _fontSize = 35;
        
        private readonly Queue<float> _fpsValues = new Queue<float>();

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
            guiStyle.fontSize = _fontSize;
            guiStyle.fontStyle = FontStyle.Bold;
            guiStyle.active = new GUIStyleState
            {
                textColor = Color.white
            };

            GUI.Label(new Rect(_xPos, _yPos, 100, 50), $"FPS:{(int)averageFPS}", guiStyle);
        }
    }
}