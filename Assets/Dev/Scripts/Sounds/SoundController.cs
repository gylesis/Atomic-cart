using System;
using Dev.Utils;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;
using Random = UnityEngine.Random;

namespace Dev.Sounds
{
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioSource _systemSoundSource;
        [SerializeField] private Transform _soundsParent;
            
        private SoundStaticDataContainer _soundStaticDataContainer;
        private ObjectPool<AudioSource> _soundPool;
        private AudioSource _mainMusicSource;            
        
        private UserSoundSettings _userUserSoundSettings;

        private MainMusicSettings MainMusicSettings => _soundStaticDataContainer.MainMusicSettings;
        public static SoundController Instance { get; private set; }
        
        private bool _isMainMusicFaded;
        
        [Inject]
        private void Construct(SoundStaticDataContainer soundStaticDataContainer, UserSoundSettings userSoundSettings)
        {
            _userUserSoundSettings = userSoundSettings;
            _soundStaticDataContainer = soundStaticDataContainer;
        }

        private void Start()
        {
            Instance = this;
            _soundPool = new ObjectPool<AudioSource>(CreateFunc, actionOnRelease: ActionOnRelease, actionOnGet: ActionOnGet, defaultCapacity: 8);
        }
   
        #region MainMusic
        public void PlayMainMusic(bool play)
        {
            TryCreateMainMusicSource();
            
            if(_mainMusicSource.isPlaying || !play)
                _mainMusicSource.Stop();
            
            if(play)
                _mainMusicSource.Play();
        }

        public void FadeMainMusic(bool fadeIn)
        {
            _isMainMusicFaded = fadeIn;
            TryCreateMainMusicSource();
        }

        private void Update()
        {
            if (_mainMusicSource != null)
            {
                _mainMusicSource.mute = _userUserSoundSettings.IsMuted;
                
                if (_mainMusicSource.isPlaying)
                {
                    float modifier = MainMusicSettings.SmoothHideVolumeCurve.Evaluate(_mainMusicSource.time / _mainMusicSource.clip.length);
                    _mainMusicSource.volume = GetVolume(_isMainMusicFaded ? MainMusicSettings.FadedVolume : MainMusicSettings.Volume, false) * modifier;
                }
                
            }
        }

        private void TryCreateMainMusicSource()
        {
            if (_mainMusicSource == null)
            {
                _mainMusicSource = _soundPool.Get();

                _mainMusicSource.clip = MainMusicSettings.Clip;
                _mainMusicSource.pitch = 1;
                _mainMusicSource.loop = true;
                
                _mainMusicSource.volume = GetVolume(MainMusicSettings.Volume, false);
                _mainMusicSource.spatialBlend = 0;
                _mainMusicSource.mute = _userUserSoundSettings.IsMuted;
            }
        }
        
        #endregion

        #region Pooling
        private AudioSource CreateFunc()
        {
            AudioSource prev = _audioSource;
            
            AudioSource instance = Instantiate(prev, _soundsParent);
            instance.playOnAwake = false;
            instance.loop = false;
            instance.volume = 0.2f;
            instance.clip = null;
            instance.mute = _userUserSoundSettings.IsMuted;
            
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
        #endregion

        public void PlaySoundAt(string soundType, Vector3 pos, float radius = 40, bool isLocal = false)
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
            audioSource.volume = GetVolume(soundStaticData.Volume, true);
            audioSource.spatialBlend = 1;
            audioSource.maxDistance = radius;
            audioSource.mute = _userUserSoundSettings.IsMuted;
            audioSource.pitch = Random.Range(1 - soundStaticData.Pitch / 100, 1 + soundStaticData.Pitch / 100f);
            
            Extensions.Delay(audioSource.clip.length, destroyCancellationToken, () => _soundPool.Release(audioSource));
        }
        
        /// <summary>
        /// Gets sound volume depends on settings volume
        /// </summary>
        /// <param name="volume">In percentage 0 - 100</param>
        /// <param name="isAdditionalSound"></param>
        /// <returns></returns>
        private float GetVolume(float volume, bool isAdditionalSound)
        {
            volume /= 100;
            return isAdditionalSound ? volume * _userUserSoundSettings.SoundVolume : volume * _userUserSoundSettings.MusicVolume;
        }
    }
}