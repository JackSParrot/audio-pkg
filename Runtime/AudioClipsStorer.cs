using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.AddressableAssets;
using JackSParrot.Utils;

[System.Serializable]
public class SFXData
{
    public string ClipName = string.Empty;
    public AssetReference ReferencedClip = null;
    public AudioMixerGroup OutputMixer = null;
    [Range(0f, 1f)]
    public float Volume = 1f;
    [Range(.3f, 3f)]
    public float Pitch = 1f;
    public bool Loop = false;
}

[CreateAssetMenu(fileName = "ClipStorer", menuName = "Audio/ClipStorer", order = 1)]
public class AudioClipsStorer : ScriptableObject
{
    [SerializeField]
    List<SFXData> _clips = new List<SFXData>();

    public SFXData GetClipByName(string clipName)
    {
        foreach (var clip in _clips)
        {
            if (string.Equals(clip.ClipName, clipName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return clip;
            }
        }
        SharedServices.GetService<ICustomLogger>()?.LogError("Trying to get a nonexistent audio clip: " + clipName);
        return null;
    }
}
