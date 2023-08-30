using System;

namespace Dev.Infrastructure
{
    [Serializable]
    public class CharacterStats
    {
        public int Health = 100;
        public float MoveSpeed = 500;
        public float SpeedLowerSpeed = 1.1f;
        public float ShootThreshold = 0.75f;
    }
}