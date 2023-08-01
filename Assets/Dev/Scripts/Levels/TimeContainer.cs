using System;
using UnityEngine;

namespace Dev
{
    [Serializable]
    public class TimeContainer
    {
        [Range(0,59)] public int Minutes;
        [Range(0,59)] public int Seconds;

        public int OverallSeconds => Minutes * 60 + Seconds;
    }
}