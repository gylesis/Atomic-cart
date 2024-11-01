using Dev.Utils;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Dev.Sounds
{
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioSource _systemSoundSource;
        [SerializeField] private Transform _soundsParent;
            
        private SoundStaticDataContainer _soundStaticDataContainer;
        private ObjectPool<AudioSource> _soundPool;
        private SaveLoadService _saveLoadService;

        public static SoundController Instance { get; private set; }
        
        private float SoundVolume => _saveLoadService.Profile.AdditionalSoundVolume;
        private float MusicVolume => _saveLoadService.Profile.MusicVolume;

        private bool IsMuted => _saveLoadService.Profile.IsVolumeMuted;
        
        
        [Inject]
        private void Construct(SoundStaticDataContainer soundStaticDataContainer, SaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
            _soundStaticDataContainer = soundStaticDataContainer;
        }

        private void Start()
        {
            Instance = this;
            _soundPool = new ObjectPool<AudioSource>(CreateFunc, actionOnRelease: ActionOnRelease, actionOnGet: ActionOnGet, defaultCapacity: 8);
        }

        private AudioSource CreateFunc()
        {
            AudioSource prev = _audioSource;
            
            AudioSource instance = Instantiate(prev, _soundsParent);
            instance.playOnAwake = false;
            instance.loop = false;
            instance.volume = 0.2f;
            instance.clip = null;
            instance.mute = IsMuted;
            
            return instance;
        }

        private void ActionOnGet(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(true);
        }

        private void ActionOnRelease(AudioSource audioSource)
        {
            audioSource.Stop();
            audioSource.gameObject.SetActive(false);
        }

        public void PlaySoundAt(string soundType, Vector3 pos, float radius, bool isLocal = false)
        {
            Play(soundType, pos, radius);
        }

        private void Play(string soundType, Vector3 pos = default, float radius = 25)
        {
            var hasSound = _soundStaticDataContainer.TryGetSoundStaticData(soundType, out var soundStaticData);

            if (hasSound == false)
            {
                AtomicLogger.Err($"No Sound Data for {soundType}");
                return;
            }
            
            var audioClip = soundStaticData.AudioClip;
            
            var audioSource = _soundPool.Get();
            audioSource.transform.position = pos;
            audioSource.clip = audioClip;
            audioSource.Play();
            audioSource.loop = false;
            audioSource.volume = soundStaticData.Volume / 100f * SoundVolume;
            audioSource.spatialBlend = 1;
            audioSource.maxDistance = radius;
            audioSource.mute = IsMuted;
            
            Extensions.Delay(audioSource.clip.length, destroyCancellationToken, () => _soundPool.Release(audioSource));
        }
        
    }
}