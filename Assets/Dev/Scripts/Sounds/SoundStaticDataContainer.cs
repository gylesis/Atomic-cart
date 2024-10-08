using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.Sounds
{
    [CreateAssetMenu(menuName = "StaticData/SoundStaticDataContainer", fileName = "SoundStaticDataContainer", order = 0)]
    public class SoundStaticDataContainer : ScriptableObject    
    {
        [SerializeField] private List<SoundStaticData> _soundDatas;
        
        public bool TryGetSoundStaticData(string soundType, out SoundStaticData soundStaticData)
        {
            soundStaticData = _soundDatas.FirstOrDefault(x => x.SoundType == soundType);
            
            return soundStaticData != null;
        }
        
        /*
        [ContextMenu("Play")]
        public void PlaySound()
        {
            var soundStaticData = GetSoundStaticData(_soundTypeToPlay);
            PlayClip(soundStaticData.AudioClip);
        }
        
        public static void PlayClip(AudioClip clip) {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] {
                    typeof(AudioClip)
                },
                null
            );
            method.Invoke(
                null,
                new object[] {
                    clip
                }
            );
        }

        public static void StopAllClips() {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "StopAllClips",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[]{},
                null
            );
            method.Invoke(
                null,
                new object[] {}
            );
        }
        */
    }
}