using JackSParrot.Utils;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JackSParrot.Services.Audio
{
    public class AudioClipHandler : MonoBehaviour
    {
        public bool IsAlive
        {
            get
            {
                return _elapsed < _duration || _looping;
            }
            set
            {
                if(value)
                {
                    return;
                }
                if(_current != null)
                {
                    _current.ReferencedClip.ReleaseAsset();
                    _current = null;
                }
                _source.Stop();
                _looping = false;
                _elapsed = 0f;
                _duration = 0f;
                _toFollow = null;
                _transform.localPosition = Vector3.zero;
                gameObject.SetActive(false);
                Id = -1;
            }
        }

        public int Id = -1;

        Transform _toFollow = null;
        Transform _transform;
        AudioSource _source = null;
        float _elapsed = 0f;
        float _duration = 0f;
        bool _looping = false;
        SFXData _current = null;

        void Awake()
        {
            _transform = transform;
            if(_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }
        }

        public void UpdateHandler(float deltaTime)
        {
            _elapsed += deltaTime;
            if(_toFollow != null)
            {
                _transform.position = _toFollow.position;
            }
        }

        public void Play(SFXData data)
        {
            _current = data;
            _duration = 9999f;
            _current.ReferencedClip.LoadAssetAsync<AudioClip>().Completed += OnLoaded;
        }

        void  OnLoaded(AsyncOperationHandle<AudioClip> handler)
        {
            if(handler.Result == null)
            {
                SharedServices.GetService<ICustomLogger>()?.LogError("Cannot load audio clip: " + _current.ClipName);
                return;
            }
            if (_current == null)
            {
                return;
            }
            gameObject.SetActive(true);
            gameObject.name = _current.ClipName;
            _source.volume = _current.Volume;
            _source.pitch = _current.Pitch;
            _source.clip = handler.Result;
            _source.loop = _current.Loop;
            _source.spatialBlend = 0f;
            _source.outputAudioMixerGroup = _current.OutputMixer;
            _source.Play();
            _toFollow = null;
            _looping = _current.Loop;
            _duration = handler.Result.length;
        }

        public void Play(SFXData data, Vector3 position)
        {
            Play(data);
            _source.spatialBlend = 1f;
            _transform.position = position;
        }

        public void Play(SFXData data, Transform parent)
        {
            Play(data);
            _source.spatialBlend = 1f;
            _transform.position = parent.position;
            _toFollow = parent;
        }
    }
}

