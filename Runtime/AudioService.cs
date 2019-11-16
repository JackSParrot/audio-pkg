using System;
using UnityEngine;

namespace JackSParrot.Services.Audio
{
    public class AudioService : IDisposable
    {
        float _volume = 1f;
        float _sfxVolume = 1f;
        float _musicVolume = 1f;

        MusicPlayer _musicPlayer;
        SFXPlayer _sfxPlayer;

        public AudioService(AudioClipsStorer storer, float volume = 1f, float sfxVolume = 1f, float musicVolume = 1f)
        {
            _sfxPlayer = new SFXPlayer(storer);
            _musicPlayer = new MusicPlayer(storer);
            Volume = volume;
            SFXVolume = sfxVolume;
            MusicVolume = musicVolume;
        }

        public void Dispose()
        {
            _sfxPlayer.Dispose();
            _musicPlayer.Dispose();
        }

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = Mathf.Clamp(value, 0f, 1f);
                _musicPlayer.Volume = _musicVolume * _volume;
                _sfxPlayer.Volume = _musicVolume * _volume;
            }
        }

        public float MusicVolume
        {
            get
            {
                return _musicVolume;
            }
            set
            {
                _musicVolume = Mathf.Clamp(value, 0f, 1f);
                _musicPlayer.Volume = _musicVolume * _volume;
            }
        }

        public float SFXVolume
        {
            get
            {
                return _sfxVolume;
            }
            set
            {
                _sfxVolume = Mathf.Clamp(value, 0f, 1f);
                _sfxPlayer.Volume = _musicVolume * _volume;
            }
        }

        public void PlayMusic(string name)
        {
            _musicPlayer.Play(name);
        }

        public void CrossFadeMusic(string clipName, float duration = 0.3f)
        {
            _musicPlayer.CrossFade(clipName, duration);
        }

        public void PlaySFX(string clipName)
        {
            _sfxPlayer.Play(clipName);
        }

        public int PlaySFX(string clipName, Transform toFollow)
        {
            return _sfxPlayer.Play(clipName, toFollow);
        }

        public int PlaySFX(string clipName, Vector3 at)
        {
            return _sfxPlayer.Play(clipName, at);
        }

        public void StopPlayingSFX(int id)
        {
            _sfxPlayer.StopPlaying(id);
        }
    }
}
