using System;

namespace Dev
{
    [Serializable]
    public class Profile
    {
        public string Nickname;
        public int Kills;
        public int Deaths;
        
        public float AdditionalSoundVolume = 0.5f;
        public float MusicVolume = 0.5f;

        public bool IsVolumeMuted = false;
    }
    
    [Serializable]
    public class PlayerPreferences
    {
        public float AdditionalSoundVolume = 0.5f;
        public float MusicVolume = 0.5f;

        public bool IsVolumeMuted = false;
    }
    
}