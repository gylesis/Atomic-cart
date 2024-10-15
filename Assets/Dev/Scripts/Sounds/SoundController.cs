using System;
using Dev.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Dev.Sounds
{
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource[] _audioSources;
        [SerializeField] private AudioSource _systemSoundSource;
        [SerializeField] private Transform _soundsParent;
            
        private SoundStaticDataContainer _soundStaticDataContainer;
        private ObjectPool<AudioSource> _soundPool;

        public static SoundController Instance { get; private set; }
        
        [Inject]
        private void Construct(SoundStaticDataContainer soundStaticDataContainer)
        {
            _soundStaticDataContainer = soundStaticDataContainer;
            Instance = this;
            _soundPool = new ObjectPool<AudioSource>(CreateFunc, actionOnRelease: ActionOnRelease, defaultCapacity: 8);
        }

        private void ActionOnRelease(AudioSource audioSource)
        {
            audioSource.Stop();
        }

        private AudioSource CreateFunc()
        {
            AudioSource prev = _audioSources[0];
            
            AudioSource instance = Instantiate(prev, _soundsParent);
            instance.playOnAwake = false;
            instance.loop = false;
            instance.volume = 0.2f;
            instance.clip = null;
            instance.mute = false;
            
            return instance;
        }

        public void PlaySoundAt(string soundType, Vector3 pos, float radius, bool isLocal = false)
        {
            Play(soundType, pos, radius);
        }

        public void PlaySystemSound(string soundType)
        {
            var hasSound = _soundStaticDataContainer.TryGetSoundStaticData(soundType, out var soundStaticData);

            if (hasSound == false)
            {
                AtomicLogger.Err($"No Sound Data for {soundType}");
                return;
            }

            var audioClip = soundStaticData.AudioClip;
            
            var audioSource = _systemSoundSource;
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.volume = soundStaticData.Volume / 100f;
            audioSource.spatialBlend = 0;
            audioSource.Play();
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
            audioSource.volume = soundStaticData.Volume / 100f;
            audioSource.spatialBlend = 1;
            audioSource.maxDistance = radius;
            
            Extensions.Delay(audioSource.clip.length, destroyCancellationToken, () => _soundPool.Release(audioSource));
        }
        
    }
}