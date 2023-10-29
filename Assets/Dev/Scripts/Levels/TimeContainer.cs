using System;
using UnityEngine;

namespace Dev.Levels
{
    [Serializable]
    public class TimeContainer
    {
        [Range(0,59)] public int Minutes;
        [Range(0,59)] public int Seconds;

        public int OverallSeconds => Minutes * 60 + Seconds;
    }
}