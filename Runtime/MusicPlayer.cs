﻿using System.Collections;
using UnityEngine;
using System;
using JackSParrot.Utils;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JackSParrot.Services.Audio
{
    public class MusicPlayer : IDisposable
    {
        AudioSource _source = null;
        AudioClipsStorer _clipStorer = null;

        float _volume = 1.0f;
        SFXData _playingClip = null;

        public MusicPlayer(AudioClipsStorer clipStorer)
        {
            if (SharedServices.GetService<CoroutineRunner>() == null)
            {
                SharedServices.RegisterService(new CoroutineRunner());
            }
            _source = new GameObject("MusicPlayer").AddComponent<AudioSource>();
            UnityEngine.Object.DontDestroyOnLoad(_source.gameObject);
            _clipStorer = clipStorer;
        }

        public void PlayMusic(string name)
        {
            if(_playingClip != null && string.Equals(name, _playingClip.ClipName, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            if(_playingClip != null)
            {
                _source.Stop();
                _playingClip.ReferencedClip.ReleaseAsset();
            }

            if(string.IsNullOrEmpty(name))
            {
                _playingClip = null;
                return;
            }
            var clip = _clipStorer.GetClipByName(name);
            _playingClip = clip;
            clip.ReferencedClip.LoadAssetAsync<AudioClip>().Completed += OnClipLoaded;
        }

        void OnClipLoaded(AsyncOperationHandle<AudioClip> handler)
        {
            if (handler.Result == null)
            {
                SharedServices.GetService<ICustomLogger>()?.LogError("Cannot load audio clip: " + _playingClip.ClipName);
                return;
            }
            if (_playingClip == null)
            {
                return;
            }
            _volume = _playingClip.Volume;
            _source.clip = handler.Result;
            _source.loop = _playingClip.Loop;
            _source.pitch = _playingClip.Pitch;
            _source.volume = _volume;
            _source.outputAudioMixerGroup = _playingClip.OutputMixer;
            _source.Play();
        }

        public void CrossFade(string name, float duration = 0.3f)
        {
            SharedServices.GetService<CoroutineRunner>().StopAllCoroutines(this);
            SharedServices.GetService<CoroutineRunner>().StartCoroutine(this, CrossFadeCoroutine(name, duration));
        }

        IEnumerator CrossFadeCoroutine(string fadeTo, float duration)
        {
            float halfDuraion = duration * 0.5f;
            SharedServices.GetService<CoroutineRunner>().StartCoroutine(this, FadeOutCoroutine(halfDuraion));
            yield return new WaitForSeconds(halfDuraion);
            PlayMusic(fadeTo);
            SharedServices.GetService<CoroutineRunner>().StartCoroutine(this, FadeInCoroutine(halfDuraion));
        }

        IEnumerator FadeOutCoroutine(float duration)
        {
            float remaining = duration;
            while(remaining > 0)
            {
                _source.volume = _volume * remaining / duration;
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        IEnumerator FadeInCoroutine(float duration)
        {
            float remaining = duration;
            while(remaining > 0)
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
                _playingClip.ReferencedClip.ReleaseAsset();
            }
            SharedServices.GetService<CoroutineRunner>().StopAllCoroutines(this);
            UnityEngine.Object.Destroy(_source.gameObject);
        }
    }
}