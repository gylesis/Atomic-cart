using System.IO;
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

        public static class LogTags
        {
            public static string Networking = "[Networking]";
            public static string Default = "[Default]";
        }

        public static class SaveLoad
        {
            public static string PlayerSaveKey = "player";
#if UNITY_EDITOR
            public static string LogSavePath = $"{Directory.GetCurrentDirectory()}/Other";
#else
            public static string LogSavePath = $"{Application.persistentDataPath}";
#endif
            
        }

        public static class SoundKeys
        {
            public static string PlayerDeathSound = "PlayerDeathSound";
        }
        
        
    }
}