using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Dev
{
    public class LightSource : MonoBehaviour
    {
        [SerializeField] private Light2D _light2D;
        public Light2D Light2D => _light2D;

        private void Reset() => _light2D = GetComponent<Light2D>();
    }
}