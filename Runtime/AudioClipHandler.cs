using System;
using JackSParrot.AddressablesEssentials;
using UnityEngine;
using UnityEngine.Audio;

namespace JackSParrot.Services
{
    public class AudioClipHandler : MonoBehaviour
    {
        [NonSerialized]
        public int Id = -1;
        [NonSerialized]
        public AudioClipData Data = null;

        public bool IsAlive = false;

        Transform    toFollow = null;
        Transform    audioTransform;
        AudioSource  source            = null;
        float        elapsed           = 0f;
        float        duration          = 0f;
        bool         looping           = false;
        private bool isFollowingTarget = false;

        private GameObject requester   = null;
        private bool       isDestroyed = false;

        void Awake()
        {
            audioTransform = transform;
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            IsAlive = false;
        }

        public void ResetHandler()
        {
            if (isDestroyed)
                return;

            Data = null;
            elapsed = 0f;
            duration = 0f;
            source.Stop();
            toFollow = null;
            looping = false;
            IsAlive = false;
            isFollowingTarget = false;
            gameObject.SetActive(false);
            audioTransform.localPosition = Vector3.zero;

            if (requester != null)
            {
                Destroy(requester);
                requester = null;
            }
        }

        public void SetOutput(AudioMixerGroup mixerGroup)
        {
            source.outputAudioMixerGroup = mixerGroup;
        }

        private void Update()
        {
            if (!IsAlive)
                return;

            elapsed += Time.deltaTime;
            if (elapsed >= duration && !looping)
            {
                ResetHandler();
                return;
            }

            if (isFollowingTarget && toFollow != null)
            {
                audioTransform.position = toFollow.position;
            }
        }

        public void Play(AudioClipData data, float volumeModifier = 1.0f)
        {
            Data = data;
            duration = 9999f;
            IsAlive = true;

            gameObject.name = Data.ClipId;
            source.volume = Data.Volume * volumeModifier;

            source.pitch = Data.Pitch;
            source.loop = Data.Loop;
            source.spatialBlend = 0f;
            toFollow = null;
            looping = Data.Loop;

            if (requester != null)
            {
                Destroy(requester);
            }

            if (data.ReferencedClip.Asset != null)
            {
                OnLoaded(data.ReferencedClip.Asset as AudioClip);
                return;
            }

            requester = new GameObject("audioRequester");
            requester.transform.SetParent(transform);

            AddressableAssetsUtility.LoadAsset<AudioClip>(data.ReferencedClip, requester, OnLoaded);
        }

        public void UpdateVolume(float volume)
        {
            source.volume = volume * Data.Volume;
        }

        void OnLoaded(AudioClip clip)
        {
            if (clip == null)
            {
                IsAlive = false;
                Debug.Assert(false);
                return;
            }

            // this is important to avoid updating before loading the clip
            gameObject.SetActive(true);

            // 0.1f is added to the duration, as very short clips can be cut otherwise
            duration = clip.length + 0.1f;
            elapsed = 0f;
            source.clip = clip;
            source.Play();
        }

        public void Play(AudioClipData data, Vector3 position)
        {
            Play(data);
            source.spatialBlend = 1f;
            audioTransform.position = position;
        }

        public void Play(AudioClipData data, Transform parent)
        {
            Play(data);
            source.spatialBlend = 1f;
            audioTransform.position = parent.position;
            toFollow = parent;
            isFollowingTarget = true;
        }
    }
}