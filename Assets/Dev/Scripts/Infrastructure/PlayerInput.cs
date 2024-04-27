﻿using UnityEngine;

namespace Dev.Infrastructure
{
    public struct PlayerInput
    {
        public Vector2 MoveDirection;
        public Vector2 LookDirection;
        
        public bool CastAbility;
        public bool ResetAbility;
        
        public int WeaponNum;
    }
}