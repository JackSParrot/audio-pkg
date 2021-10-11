using System;
using UnityEngine;

namespace JackSParrot.Services.Audio
{
    public class AudioService : IDisposable
    {
        float _volume      = 1f;
        float _sfxVolume   = 1f;
        float _musicVolume = 1f;

        MusicPlayer      _musicPlayer;
        SFXPlayer        _sfxPlayer;
        AudioClipsStorer _clips;

        public AudioService(AudioClipsStorer storer, float volume = 1f, float sfxVolume = 1f, float musicVolume = 1f)
        {
            _clips = storer;
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
            get { return _volume; }
            set
            {
                _volume = Mathf.Clamp(value, 0f, 1f);
                _musicPlayer.Volume = _musicVolume * _volume;
                _sfxPlayer.Volume = _sfxVolume * _volume;
            }
        }

        public float MusicVolume
        {
            get { return _musicVolume; }
            set
            {
                _musicVolume = Mathf.Clamp(value, 0f, 1f);
                _musicPlayer.Volume = _musicVolume * _volume;
            }
        }

        public float SFXVolume
        {
            get { return _sfxVolume; }
            set
            {
                _sfxVolume = Mathf.Clamp(value, 0f, 1f);
                _sfxPlayer.Volume = _sfxVolume * _volume;
            }
        }

        public void PlayMusic(ClipId clipId)
        {
            _musicPlayer.Play(clipId);
        }

        public void CrossFadeMusic(ClipId clipId, float duration = 0.3f)
        {
            _musicPlayer.CrossFade(clipId, duration);
        }

        public void PlaySFX(ClipId clipId)
        {
            _sfxPlayer.Play(clipId);
        }

        public int PlaySFX(ClipId clipId, Transform toFollow)
        {
            return _sfxPlayer.Play(clipId, toFollow);
        }

        public int PlaySFX(ClipId clipId, Vector3 at)
        {
            return _sfxPlayer.Play(clipId, at);
        }

        public void StopPlayingSFX(int id)
        {
            _sfxPlayer.StopPlaying(id);
        }

        public void LoadClipsForCategory(string categoryId) => _clips.LoadClipsForCategory(categoryId);
        public void UnloadClipsForCategory(string categoryId) => _clips.UnloadClipsForCategory(categoryId);
    }
}