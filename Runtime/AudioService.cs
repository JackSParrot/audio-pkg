using System;
using UnityEngine;
using UnityEngine.Audio;

namespace JackSParrot.Services.Audio
{
    public class AudioService : IDisposable
    {
        float                _volume = 1f;
        MusicPlayer          _musicPlayer;
        SfxPlayer            _sfxPlayer;
        AudioClipsStorer     _clips;
        AudioMixerGroup      _masterGroup;
        private UpdateRunner _updateRunner;

        public AudioService(AudioClipsStorer storer, float volume = 1f, float sfxVolume = 1f, float musicVolume = 1f)
        {
            _clips = storer;
            _updateRunner = new GameObject("AudioServiceUpdater", typeof(UpdateRunner)).GetComponent<UpdateRunner>();
            UnityEngine.Object.DontDestroyOnLoad(_updateRunner);

            AudioMixer mixer = Resources.Load<AudioMixer>("GameAudioMixer");
            if (mixer == null)
            {
                mixer = Resources.Load<AudioMixer>("JackSParrotMixer");
            }

            var musicGroup = mixer.FindMatchingGroups("Music")[0];
            var sfxGroup = mixer.FindMatchingGroups("SFX")[0];
            _masterGroup = mixer.FindMatchingGroups("Master")[0];
            _sfxPlayer = new SfxPlayer(storer, sfxGroup, _updateRunner);
            _musicPlayer = new MusicPlayer(storer, musicGroup, _updateRunner);
            Volume = volume;
            SfxVolume = sfxVolume;
            MusicVolume = musicVolume;
        }

        public void Dispose()
        {
            _sfxPlayer.Dispose();
            _musicPlayer.Dispose();
            UnityEngine.Object.Destroy(_updateRunner);
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Mathf.Clamp(value, 0.001f, 1f);
                _masterGroup.audioMixer.SetFloat("masterVolume", Mathf.Log10(_volume) * 20f);
            }
        }

        public float MusicVolume
        {
            get => _musicPlayer.Volume;
            set => _musicPlayer.Volume = value;
        }

        public float SfxVolume
        {
            get => _sfxPlayer.Volume;
            set => _sfxPlayer.Volume = value;
        }

        public void PlayMusic(ClipId clipId)
        {
            _musicPlayer.Play(clipId);
        }

        public void CrossFadeMusic(ClipId clipId, float duration = 0.3f)
        {
            _musicPlayer.CrossFade(clipId, duration);
        }

        public void PlaySfx(ClipId clipId)
        {
            _sfxPlayer.Play(clipId);
        }

        public int PlaySfx(ClipId clipId, Transform toFollow)
        {
            return _sfxPlayer.Play(clipId, toFollow);
        }

        public int PlaySfx(ClipId clipId, Vector3 at)
        {
            return _sfxPlayer.Play(clipId, at);
        }

        public void StopPlayingSfx(int id)
        {
            _sfxPlayer.StopPlaying(id);
        }

        public void LoadClipsForCategory(string categoryId) => _clips.LoadClipsForCategory(categoryId);
        public void UnloadClipsForCategory(string categoryId) => _clips.UnloadClipsForCategory(categoryId);

        internal class UpdateRunner : MonoBehaviour
        {
            public Action<float> OnUpdate = t => { };

            private void Update() => OnUpdate(Time.deltaTime);
        }
    }
}