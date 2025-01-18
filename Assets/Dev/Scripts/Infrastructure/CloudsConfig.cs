using System;
using System.Collections.Generic;
using Dev.Levels.Env;
using UnityEngine;

namespace Dev.Infrastructure
{
    [Serializable]
    public class CloudsConfig
    {
        public Cloud CloudPrefab;
        public List<Sprite> CloudsSprites = new List<Sprite>();
    }
}