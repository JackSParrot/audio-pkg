using System;
using UnityEngine;
using UnityEngine.Audio;

namespace JackSParrot.Services.Audio
{
    public class AudioManagerBehaviour : MonoBehaviour
    {
        [SerializeField]
        AudioMixer _audioMixer = null;

        [SerializeField]
        AudioMixerGroup _musicOutput = null;

        [SerializeField]
        AudioMixerGroup _fxOutput = null;

        float _volume = 1f;
        float _sfxVolume = 1f;
        float _musicVolume = 1f;

        float VolumeToAttenuation(float val)
        {
            return Mathf.Log10(Mathf.Clamp(val, 0.0001f, 1f)) * 20f;
        }

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = Mathf.Clamp(value, 0.0001f, 1f);
                _audioMixer.SetFloat("master_volume", VolumeToAttenuation(_volume));
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
                _musicVolume = Mathf.Clamp(value, 0.00001f, 1f);
                _audioMixer.SetFloat("music_volume", VolumeToAttenuation(_musicVolume));
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
                _sfxVolume = Mathf.Clamp(value, 0.00001f, 1f);
                _audioMixer.SetFloat("sfx_volume", VolumeToAttenuation(_sfxVolume));
            }
        }
    }

    public class AudioManager : IDisposable
    {
        static AudioManagerBehaviour _instance;

        public float Volume
        {
            get
            {
                return _instance.Volume;
            }
            set
            {
                _instance.Volume = value;
            }
        }

        public float SFXVolume
        {
            get
            {
                return _instance.SFXVolume;
            }
            set
            {
                _instance.SFXVolume = value;
            }
        }

        public float MusicVolume
        {
            get
            {
                return _instance.MusicVolume;
            }
            set
            {
                _instance.MusicVolume = value;
            }
        }

        public AudioManager()
        {
            _instance = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("AudioManager")).GetComponent<AudioManagerBehaviour>();
            UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_instance.gameObject);
        }
    }
}