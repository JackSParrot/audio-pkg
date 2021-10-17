#if UNITY_EDITOR
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("JackSParrotAudio.Editor")]
namespace JackSParrot.Audio.Editor
{
    public static class EditorUtils
    {
        internal static AudioClipsStorer GetOrCreateAudioClipsStorer()
        {
            AudioClipsStorer retVal = null;
            var clips = AssetDatabase.FindAssets("t: AudioClipsStorer");
            if (clips.Length < 1)
            {
                AudioClipsStorer item = ScriptableObject.CreateInstance<AudioClipsStorer>();
                AssetDatabase.CreateAsset(item, "Assets/AudioClipStorer.asset");
                return item;
            }

            if (clips.Length > 1)
            {
                Debug.LogError("There is a duplicate AudioClipStorer");
            }

            AudioClipsStorer storer =
                AssetDatabase.LoadAssetAtPath<AudioClipsStorer>(AssetDatabase.GUIDToAssetPath(clips[0]));
            return storer;
        }
    }
}
#endif