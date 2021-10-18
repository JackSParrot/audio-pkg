using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace JackSParrot.Audio
{
    [CreateAssetMenu(fileName = "ClipStorer", menuName = "Audio/ClipStorer", order = 1)]
    public class AudioClipsStorer : ScriptableObject
    {
        [NonSerialized]
        public AudioService AudioService;

        public AudioMixer OutputMixer = null;

        [SerializeField]
        List<AudioCategory> _categories = new List<AudioCategory>();

        public IReadOnlyList<AudioCategory> Categories => _categories;

        public List<string> GetAllClips()
        {
            var retVal = new List<string>();
            foreach (var category in _categories)
            {
                foreach (var clip in category.Clips)
                {
                    retVal.Add(clip.ClipId);
                }
            }

            return retVal;
        }

        public AudioClipData GetClipById(ClipId clipId)
        {
            foreach (var category in _categories)
            {
                foreach (var clip in category.Clips)
                {
                    if (clip.ClipId == clipId)
                    {
                        return clip;
                    }
                }
            }

            Debug.Assert(false);
            return null;
        }

        public void LoadClipsForCategory(string categoryId)
        {
            var category = GetCategoryById(categoryId);
            if (category == null)
            {
                Debug.Assert(false);
                return;
            }

            foreach (var clip in category.Clips)
            {
                if (clip.ReferencedClip.IsValid() && clip.ReferencedClip.IsDone)
                {
                    clip.ReferencedClip.LoadAssetAsync<AudioClip>();
                }
            }
        }

        public void UnloadClipsForCategory(string categoryId)
        {
            var category = GetCategoryById(categoryId);
            if (category == null)
            {
                Debug.Assert(false);
                return;
            }

            foreach (var clip in category.Clips)
            {
                if (clip.ReferencedClip.IsValid() && clip.ReferencedClip.IsDone)
                {
                    clip.ReferencedClip.ReleaseAsset();
                }
            }
        }

        public AudioCategory GetCategoryById(string categoryId)
        {
            foreach (var category in _categories)
            {
                if (categoryId.Equals(category.Id))
                {
                    return category;
                }
            }

            return null;
        }
    }
}