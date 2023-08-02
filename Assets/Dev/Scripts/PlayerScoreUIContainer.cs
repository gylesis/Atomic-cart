﻿using Dev.Infrastructure;
using UnityEngine;

namespace Dev
{
    public class PlayerScoreUIContainer : NetworkContext
    {
        [SerializeField] private Transform _scoresUIParent;

        public Transform ScoresUIParent => _scoresUIParent;
    }
}