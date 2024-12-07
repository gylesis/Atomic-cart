using Dev.Infrastructure;
using Zenject;

namespace Dev.Sounds
{
    public class SoundSettings
    {
        private AuthService _authService;

        public float SoundVolume => _authService.MyProfile.AdditionalSoundVolume;
        public float MusicVolume => _authService.MyProfile.MusicVolume;
        public bool IsMuted => _authService.MyProfile.IsVolumeMuted;
        
        [Inject]
        private void Construct(AuthService authService)
        {
            _authService = authService;
        }
    }
}