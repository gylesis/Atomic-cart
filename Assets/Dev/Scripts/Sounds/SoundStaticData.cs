using System;
using UnityEngine;

namespace Dev.Sounds
{
    [Serializable]
    public class SoundStaticData
    {
        public string SoundType;
        public AudioClip AudioClip;
        [Range(0f,100f)] public float Volume;
    }
}