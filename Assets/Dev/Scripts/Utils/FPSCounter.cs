using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Dev.Utils
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _fpsText;

        private readonly Queue<float> _fpsValues = new Queue<float>();

        private void Update()
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

            _fpsText.text = $"FPS:{(int)averageFPS}";
        }
    }
}