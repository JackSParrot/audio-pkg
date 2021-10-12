using System.Collections;
using UnityEngine;
using System;
using JackSParrot.Utils;
using UnityEngine.Audio;

namespace JackSParrot.Services.Audio
{
    public class MusicPlayer : IDisposable
    {
        float _volume = 1f;
        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = Mathf.Clamp(value, 0.0001f, 1f);
                _outputMixerGroup.audioMixer.SetFloat("musicVolume", Mathf.Log10(_volume) * 20f);
            }
        }

        AudioSource      _source     = null;
        AudioClipsStorer _clipStorer = null;

        AudioClipData           _playingClip      = null;
        private AudioMixerGroup _outputMixerGroup = null;

        internal MusicPlayer(AudioClipsStorer clipStorer, AudioMixerGroup outputGroup)
        {
            if (!SharedServices.HasService<ICoroutineRunner>())
            {
                SharedServices.RegisterService<ICoroutineRunner>(new CoroutineRunner());
            }

            _outputMixerGroup = outputGroup;
            _clipStorer = clipStorer;
            _source = new GameObject("MusicPlayer").AddComponent<AudioSource>();
            UnityEngine.Object.DontDestroyOnLoad(_source.gameObject);
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.outputAudioMixerGroup = _outputMixerGroup;
        }

        public void Play(ClipId clipId)
        {
            if (_playingClip != null && clipId == _playingClip.ClipId)
            {
                return;
            }

            if (_playingClip != null)
            {
                _source.Stop();
                _playingClip.ReferencedClip.ReleaseAsset();
            }

            var clip = _clipStorer.GetClipById(clipId);
            _playingClip = clip;
            clip.ReferencedClip.LoadAssetAsync<AudioClip>().Completed += h => OnClipLoaded(h.Result);
        }

        void OnClipLoaded(AudioClip clip)
        {
            if (clip == null)
            {
                SharedServices.GetService<ICustomLogger>()?.LogError("Cannot load audio clip: " + _playingClip.ClipId);
                return;
            }

            _source.clip = clip;
            _source.loop = _playingClip.Loop;
            _source.pitch = _playingClip.Pitch;
            _source.volume = _playingClip.Volume;
            _source.Play();
        }

        public void CrossFade(ClipId clipId, float duration)
        {
            SharedServices.GetService<CoroutineRunner>().StopAllCoroutines(this);
            SharedServices.GetService<CoroutineRunner>().StartCoroutine(this, CrossFadeCoroutine(clipId, duration));
        }

        IEnumerator CrossFadeCoroutine(ClipId fadeTo, float duration)
        {
            float halfDuraion = duration * 0.5f;
            SharedServices.GetService<CoroutineRunner>().StartCoroutine(this, FadeOutCoroutine(halfDuraion));
            yield return new WaitForSeconds(halfDuraion);
            Play(fadeTo);
            SharedServices.GetService<CoroutineRunner>().StartCoroutine(this, FadeInCoroutine(halfDuraion));
        }

        IEnumerator FadeOutCoroutine(float duration)
        {
            float remaining = duration;
            while (remaining > 0)
            {
                _source.volume = _volume * remaining / duration;
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        IEnumerator FadeInCoroutine(float duration)
        {
            float remaining = duration;
            while (remaining > 0)
            {
                _source.volume = _volume * (1f - (remaining / duration));
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        public void Dispose()
        {
            if (_playingClip != null)
            {
                _source.Stop();
                if (_playingClip.ReferencedClip.Asset != null)
                {
                    _playingClip.ReferencedClip.ReleaseAsset();
                }
            }

            SharedServices.GetService<CoroutineRunner>()?.StopAllCoroutines(this);
            UnityEngine.Object.Destroy(_source.gameObject);
        }
    }
}