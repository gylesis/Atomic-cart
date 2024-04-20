using Dev.PlayerLogic;
using UnityEngine;

namespace Dev.Utils
{
    public static class AtomicConstants
    {

        public static class Teams
        {
            public static Color GetTeamColor(TeamSide teamSide) => teamSide == TeamSide.Blue ? Color.blue : Color.red;
        }
        public static class DamageIds
        {
            public static int BotDamageId = 3;
            public static int ObstacleDamageId = -1;
            public static int ObstacleWithHealthDamageId = 0;
            public static int DummyTargetDamageId = -2;
        }

    }
}