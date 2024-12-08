using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev
{
    [CreateAssetMenu(menuName = "CameraShakeConfig", fileName = "StaticData/CameraShakeConfig", order = 0)]
    public class CameraShakeConfig : ScriptableObject
    {
        [SerializeField] private List<ShakeData> _shakeDatas;

        public ShakeData GetData(string key) => _shakeDatas.FirstOrDefault(x => x.Name == key);
    }
    
    [Serializable]
    public class ShakeData
    {
        public string Name = "shakey-shakey";
        [Range(0.05f, 2f)] public float Duration = 0.3f;
        [Range(0.1f, 1f)] public float Power = 1;
        [Range(10, 100)] public int Vibrato = 50;
    }
}